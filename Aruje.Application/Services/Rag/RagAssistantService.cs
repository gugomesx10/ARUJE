using Aruje.Application.DTOs.Rag;
using Aruje.Application.Interfaces.Services;


namespace Aruje.Application.Services.Rag;

public class RagAssistantService : IRagAssistantService
{
    private readonly IRagContextBuilder _contextBuilder;
    private readonly IRagPromptBuilder _promptBuilder;
    private readonly IRagLlmProvider _llmProvider;
    private readonly IRagIntentClassifier _intentClassifier;

    public RagAssistantService(
        IRagContextBuilder contextBuilder,
        IRagPromptBuilder promptBuilder,
        IRagLlmProvider llmProvider,
        IRagIntentClassifier intentClassifier)
    {
        _contextBuilder = contextBuilder;
        _promptBuilder = promptBuilder;
        _llmProvider = llmProvider;
        _intentClassifier = intentClassifier;
    }

    public async Task<RagAskResponse> AskAsync(
        RagAskRequest request,
        CancellationToken cancellationToken = default)
    {
        var question = request.Question.Trim();
        var maxItems = Math.Clamp(request.MaxItems, 5, 20);

        var conversationHistory =
            request.ConversationHistory ?? Array.Empty<RagConversationMessageRequest>();

        var intentResult = _intentClassifier.Classify(
            question,
            conversationHistory
        );

        if (!intentResult.ShouldUseRag)
        {
            return new RagAskResponse(
                question,
                intentResult.DirectAnswer ??
                "Eu consigo te ajudar com dúvidas sobre lavoura, sensores, leituras, alertas e recomendações agrícolas.",
                "Indefinido",
                "Faça uma pergunta relacionada à lavoura, alertas, sensores ou recomendações do Arujé.",
                "Aruje-Intent-RuleBased",
                Array.Empty<RagSourceResponse>(),
                DateTime.UtcNow
            );
        }

        var context = await _contextBuilder.BuildAsync(
            question,
            maxItems,
            cancellationToken
        );

        var prompt = BuildPromptWithConversationHistory(
            context,
            conversationHistory
        );

        var llmResponse = await _llmProvider.GenerateAsync(
            prompt,
            context,
            cancellationToken
        );

        var sources = context.Items
            .Select(item =>
                new RagSourceResponse(
                    item.Type,
                    item.Id,
                    item.Title,
                    BuildSummary(item.Content),
                    item.RelevanceScore,
                    item.CreatedAt
                )
            )
            .ToList();

        return new RagAskResponse(
            question,
            llmResponse.Answer,
            llmResponse.RiskLevel,
            llmResponse.Recommendation,
            llmResponse.Provider,
            sources,
            DateTime.UtcNow
        );
    }

    private string BuildPromptWithConversationHistory(
        RagContext context,
        IReadOnlyList<RagConversationMessageRequest> conversationHistory)
    {
        var prompt = _promptBuilder.BuildPrompt(context);

        if (!conversationHistory.Any())
            return prompt;

        var recentMessages = conversationHistory
            .Where(message =>
                !string.IsNullOrWhiteSpace(message.Role) &&
                !string.IsNullOrWhiteSpace(message.Content))
            .TakeLast(6)
            .ToList();

        if (!recentMessages.Any())
            return prompt;

        var historyBuilder = new System.Text.StringBuilder();

        historyBuilder.AppendLine();
        historyBuilder.AppendLine("Histórico recente da conversa:");
        historyBuilder.AppendLine("Use este histórico apenas para entender referências como 'isso', 'esse alerta', 'essa leitura' ou 'o que faço agora'.");
        historyBuilder.AppendLine("Não invente fatos com base no histórico. Os fatos confiáveis são os dados do contexto recuperado.");
        historyBuilder.AppendLine();

        foreach (var message in recentMessages)
        {
            var role = message.Role.Trim().ToLowerInvariant() switch
            {
                "user" => "Usuário",
                "assistant" => "Arujé IA",
                _ => "Mensagem"
            };

            historyBuilder.AppendLine($"{role}: {message.Content.Trim()}");
        }

        return prompt + historyBuilder.ToString();
    }
    
    private static string BuildSummary(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        const int maxLength = 260;

        if (content.Length <= maxLength)
            return content;

        return content[..maxLength].Trim() + "...";
    }
}