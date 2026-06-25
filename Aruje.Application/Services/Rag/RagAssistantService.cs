using Aruje.Application.DTOs.Rag;
using Aruje.Application.Interfaces.Services;

namespace Aruje.Application.Services.Rag;

public class RagAssistantService : IRagAssistantService
{
    private readonly IRagContextBuilder _contextBuilder;
    private readonly IRagPromptBuilder _promptBuilder;
    private readonly IRagLlmProvider _llmProvider;

    public RagAssistantService(
        IRagContextBuilder contextBuilder,
        IRagPromptBuilder promptBuilder,
        IRagLlmProvider llmProvider)
    {
        _contextBuilder = contextBuilder;
        _promptBuilder = promptBuilder;
        _llmProvider = llmProvider;
    }

    public async Task<RagAskResponse> AskAsync(
        RagAskRequest request,
        CancellationToken cancellationToken = default)
    {
        var question = request.Question.Trim();
        var maxItems = Math.Clamp(request.MaxItems, 5, 20);

        var context = await _contextBuilder.BuildAsync(
            question,
            maxItems,
            cancellationToken
        );

        var prompt = _promptBuilder.BuildPrompt(context);

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