namespace Aruje.Application.DTOs.AiAnalyses;

public record AiAnalysisResponse(
    Guid Id,
    Guid AlertId,
    string RiskLevel,
    string Reason,
    string Recommendation,
    string Provider,
    DateTime CreatedAt
);