namespace Aruje.Application.DTOs.Rag;

public record RagIntentResult(
    RagIntentType Intent,
    bool ShouldUseRag,
    string? DirectAnswer
);