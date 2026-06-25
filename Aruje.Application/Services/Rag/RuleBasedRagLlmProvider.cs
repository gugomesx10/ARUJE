using Aruje.Application.DTOs.Rag;
using Aruje.Application.Interfaces.Services;

namespace Aruje.Application.Services.Rag;

public class RuleBasedRagLlmProvider : IRagLlmProvider
{
    public Task<RagLlmResponse> GenerateAsync(
        string prompt,
        RagContext context,
        CancellationToken cancellationToken = default)
    {
        _ = prompt;

        if (!context.Items.Any())
        {
            return Task.FromResult(
                new RagLlmResponse(
                    "Não encontrei dados suficientes no Arujé para responder com segurança. É necessário gerar leituras, alertas ou análises antes de tomar uma decisão.",
                    "Indefinido",
                    "Execute a simulação IoT, aguarde o processamento do Worker e consulte novamente o assistente após a geração de leituras e alertas.",
                    "Aruje-RAG-RuleBased"
                )
            );
        }

        var riskLevel = DetermineRiskLevel(context);
        var recommendation = BuildRecommendation(context, riskLevel);
        var answer = BuildAnswer(context, riskLevel, recommendation);

        return Task.FromResult(
            new RagLlmResponse(
                answer,
                riskLevel,
                recommendation,
                "Aruje-RAG-RuleBased"
            )
        );
    }

    private static string DetermineRiskLevel(RagContext context)
    {
        var text = BuildSearchableText(context);

        if (ContainsAny(
                text,
                "severidade: high",
                "risco high",
                "nivel de risco: high",
                "nível de risco: high",
                "risco alto",
                "severidade: alto"))
        {
            return "Alto";
        }

        if (ContainsAny(
                text,
                "severidade: medium",
                "risco medium",
                "nivel de risco: medium",
                "nível de risco: medium",
                "risco medio",
                "risco médio",
                "severidade: medio",
                "severidade: médio"))
        {
            return "Médio";
        }

        if (ContainsAny(
                text,
                "temperatura: 38",
                "temperatura: 39",
                "temperatura: 40",
                "umidade do solo: 20",
                "umidade do solo: 19",
                "umidade do solo: 18"))
        {
            return "Alto";
        }

        if (ContainsAny(
                text,
                "temperatura: 35",
                "temperatura: 36",
                "temperatura: 37",
                "umidade do solo: 25",
                "umidade do solo: 24",
                "umidade do solo: 23",
                "umidade do solo: 22",
                "umidade do solo: 21"))
        {
            return "Médio";
        }

        return "Baixo";
    }

    private static string BuildRecommendation(
        RagContext context,
        string riskLevel)
    {
        var latestAnalysis = context.Items
            .Where(x => x.Type == "AiAnalysis")
            .OrderByDescending(x => x.RelevanceScore)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (latestAnalysis is not null &&
            latestAnalysis.Content.Contains("Recomendação:", StringComparison.OrdinalIgnoreCase))
        {
            return ExtractRecommendation(latestAnalysis.Content);
        }

        var text = BuildSearchableText(context);

        if (riskLevel == "Alto")
        {
            if (ContainsAny(
                    text,
                    "baixa umidade",
                    "umidade do solo",
                    "seca",
                    "irrigacao",
                    "irrigação"))
            {
                return "Priorizar a verificação da umidade do solo, avaliar irrigação preventiva e acompanhar as próximas leituras para confirmar se o risco foi reduzido.";
            }

            if (ContainsAny(
                    text,
                    "temperatura elevada",
                    "temperatura",
                    "calor",
                    "estresse termico",
                    "estresse térmico"))
            {
                return "Verificar a plantação em campo, acompanhar a temperatura nas próximas leituras e avaliar ações para reduzir estresse térmico na cultura.";
            }

            return "Verificar a plantação com prioridade, analisar os alertas recentes e acompanhar novas leituras antes de considerar o risco estabilizado.";
        }

        if (riskLevel == "Médio")
        {
            return "Acompanhar a evolução das próximas leituras, manter monitoramento ativo e agir caso temperatura, umidade ou alertas piorem.";
        }

        return "Manter o monitoramento preventivo da lavoura e acompanhar novas leituras para identificar mudanças no cenário.";
    }

    private static string BuildAnswer(
        RagContext context,
        string riskLevel,
        string recommendation)
    {
        var latestReading = context.Items
            .Where(x => x.Type == "SensorReading")
            .OrderByDescending(x => x.RelevanceScore)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        var latestAlert = context.Items
            .Where(x => x.Type == "Alert")
            .OrderByDescending(x => x.RelevanceScore)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        var latestAnalysis = context.Items
            .Where(x => x.Type == "AiAnalysis")
            .OrderByDescending(x => x.RelevanceScore)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        var totalReadings = context.Items.Count(x => x.Type == "SensorReading");
        var totalAlerts = context.Items.Count(x => x.Type == "Alert");
        var totalAnalyses = context.Items.Count(x => x.Type == "AiAnalysis");

        var answerParts = new List<string>
        {
            $"Com base nos dados recuperados pelo Arujé, o nível de risco atual é {riskLevel}."
        };

        if (latestReading is not null)
        {
            answerParts.Add(
                $"A leitura mais relevante encontrada indica: {latestReading.Content}"
            );
        }

        if (latestAlert is not null)
        {
            answerParts.Add(
                $"O alerta mais relevante é \"{latestAlert.Title}\", com o seguinte contexto: {latestAlert.Content}"
            );
        }

        if (latestAnalysis is not null)
        {
            answerParts.Add(
                $"A análise inteligente mais relevante complementa o diagnóstico: {latestAnalysis.Content}"
            );
        }

        answerParts.Add(
            $"Foram consideradas {totalReadings} leitura(s), {totalAlerts} alerta(s) e {totalAnalyses} análise(s) inteligente(s) como fontes de contexto."
        );

        answerParts.Add($"Recomendação: {recommendation}");

        return string.Join(" ", answerParts);
    }

    private static string ExtractRecommendation(string content)
    {
        var marker = "Recomendação:";
        var index = content.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            return "Acompanhar as próximas leituras e verificar os alertas recentes antes de tomar uma decisão.";
        }

        var recommendation = content[(index + marker.Length)..].Trim();

        var providerIndex = recommendation.IndexOf(
            "Provider:",
            StringComparison.OrdinalIgnoreCase
        );

        if (providerIndex >= 0)
        {
            recommendation = recommendation[..providerIndex].Trim();
        }

        return string.IsNullOrWhiteSpace(recommendation)
            ? "Acompanhar as próximas leituras e verificar os alertas recentes antes de tomar uma decisão."
            : recommendation;
    }

    private static string BuildSearchableText(RagContext context)
    {
        return string.Join(
                " ",
                context.Items.Select(item => $"{item.Type} {item.Title} {item.Content}")
            )
            .ToLowerInvariant();
    }

    private static bool ContainsAny(
        string text,
        params string[] terms)
    {
        return terms.Any(term => text.Contains(term.ToLowerInvariant()));
    }
}