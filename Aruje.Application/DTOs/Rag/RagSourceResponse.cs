namespace Aruje.Application.DTOs.Rag;

public record RagSourceResponse(
    string Type,
    Guid? Id,
    string Title,
    string Summary,
    decimal RelevanceScore,
    DateTime CreatedAt
);