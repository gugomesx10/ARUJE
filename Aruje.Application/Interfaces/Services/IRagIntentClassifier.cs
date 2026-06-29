using Aruje.Application.DTOs.Rag;

namespace Aruje.Application.Interfaces.Services;

public interface IRagIntentClassifier
{
    RagIntentResult Classify(
        string question,
        IReadOnlyList<RagConversationMessageRequest> conversationHistory);
}