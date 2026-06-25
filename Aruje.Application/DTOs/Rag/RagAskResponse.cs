namespace Aruje.Application.DTOs.Rag;

public record RagAskResponse(
    string Question,
    string Answer,
    string RiskLevel,
    string Recommendation,
    string Provider,
    IReadOnlyList<RagSourceResponse> Sources,
    DateTime GeneratedAt
);