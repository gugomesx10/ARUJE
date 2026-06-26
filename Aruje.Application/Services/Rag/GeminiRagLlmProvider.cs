using System.Net.Http.Json;
using System.Text.Json;
using Aruje.Application.DTOs.Rag;
using Aruje.Application.Interfaces.Services;

namespace Aruje.Application.Services.Rag;

public class GeminiRagLlmProvider : IRagLlmProvider
{
    private readonly RuleBasedRagLlmProvider _fallbackProvider = new();

    public async Task<RagLlmResponse> GenerateAsync(
        string prompt,
        RagContext context,
        CancellationToken cancellationToken = default)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return await _fallbackProvider.GenerateAsync(
                prompt,
                context,
                cancellationToken
            );
        }

        try
        {
            var model = Environment.GetEnvironmentVariable("GEMINI_MODEL");

            if (string.IsNullOrWhiteSpace(model))
                model = "gemini-2.5-flash";

            if (!model.StartsWith("models/"))
                model = $"models/{model}";

            var endpoint =
                $"https://generativelanguage.googleapis.com/v1beta/{model}:generateContent";

            var finalPrompt =
                "Você é a Arujé IA, um assistente virtual agrícola.\n" +
                "Responda em português do Brasil, com linguagem simples, humana, acessível e acolhedora.\n" +
                "Use apenas o contexto recebido. Não invente sensores, leituras, alertas ou recomendações.\n" +
                "Ajude pessoas que talvez tenham dificuldade para explicar problemas técnicos.\n" +
                "Não exponha IDs técnicos na resposta principal.\n" +
                "Seja breve. O campo answer deve ter no máximo 4 frases.\n" +
                "O campo recommendation deve ter no máximo 2 frases.\n" +
                "Não escreva JSON dentro do campo answer.\n" +
                "Não use markdown.\n" +
                "Não use bloco de código.\n" +
                "Retorne uma resposta em JSON válido, sem markdown, sem bloco de código e sem texto fora do JSON.\n" +
                "O JSON deve ter exatamente estes campos:\n" +
                "- answer: resposta amigável e acessível\n" +
                "- riskLevel: Alto, Médio, Baixo ou Indefinido\n" +
                "- recommendation: ação prática recomendada\n\n" +
                "Contexto recuperado do Arujé:\n" +
                prompt;
            
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text = finalPrompt
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    topP = 0.8,
                    maxOutputTokens = 2048,
                    responseMimeType = "application/json"
                }
            };

            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            
            httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

            var response = await httpClient.PostAsJsonAsync(
                endpoint,
                payload,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                return await _fallbackProvider.GenerateAsync(
                    prompt,
                    context,
                    cancellationToken
                );
            }

            var responseJson = await response.Content.ReadAsStringAsync(
                cancellationToken
            );

            var geminiText = ExtractText(responseJson);

            if (string.IsNullOrWhiteSpace(geminiText))
            {
                return await _fallbackProvider.GenerateAsync(
                    prompt,
                    context,
                    cancellationToken
                );
            }

            var parsed = TryParseResponse(geminiText);

            if (parsed is not null)
                return parsed;

            return await _fallbackProvider.GenerateAsync(
                prompt,
                context,
                cancellationToken
            );
        }
        catch
        {
            return await _fallbackProvider.GenerateAsync(
                prompt,
                context,
                cancellationToken
            );
        }
    }
    
        private static string ExtractText(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);

        if (!document.RootElement.TryGetProperty("candidates", out var candidates))
            return string.Empty;

        foreach (var candidate in candidates.EnumerateArray())
        {
            if (!candidate.TryGetProperty("content", out var content))
                continue;

            if (!content.TryGetProperty("parts", out var parts))
                continue;

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text))
                    return text.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static RagLlmResponse? TryParseResponse(string geminiText)
    {
        try
        {
            var json = CleanJson(geminiText);

            using var document = JsonDocument.Parse(json);

            var root = document.RootElement;

            var answer = GetString(root, "answer");
            var riskLevel = GetString(root, "riskLevel");
            var recommendation = GetString(root, "recommendation");

            if (string.IsNullOrWhiteSpace(answer))
                return null;

            if (string.IsNullOrWhiteSpace(riskLevel))
                riskLevel = "Indefinido";

            if (string.IsNullOrWhiteSpace(recommendation))
                recommendation = "Acompanhar os dados da lavoura e verificar os alertas recentes.";

            return new RagLlmResponse(
                answer.Trim(),
                NormalizeRiskLevel(riskLevel),
                recommendation.Trim(),
                "Gemini-RAG"
            );
        }
        catch
        {
            return null;
        }
    }

    private static string CleanJson(string value)
    {
        var clean = value.Trim();

        clean = clean
            .Replace("```json", string.Empty)
            .Replace("```", string.Empty)
            .Trim();

        var firstBrace = clean.IndexOf('{');
        var lastBrace = clean.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
            clean = clean[firstBrace..(lastBrace + 1)];

        return clean;
    }

    private static string? GetString(
        JsonElement root,
        string propertyName)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                return property.Value.GetString();
        }

        return null;
    }

    private static string NormalizeRiskLevel(string riskLevel)
    {
        var normalized = riskLevel.Trim().ToLowerInvariant();

        if (normalized.Contains("alto"))
            return "Alto";

        if (normalized.Contains("médio") || normalized.Contains("medio"))
            return "Médio";

        if (normalized.Contains("baixo"))
            return "Baixo";

        return "Indefinido";
    }
}
                
                