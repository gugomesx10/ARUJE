namespace Aruje.Application.DTOs.Rag;

public record RagLlmResponse(
    string Answer,
    string RiskLevel,
    string Recommendation,
    string Provider
);