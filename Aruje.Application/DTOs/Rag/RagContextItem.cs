namespace Aruje.Application.DTOs.Rag;

public record RagContextItem(
    string Type,
    Guid? Id,
    string Title,
    string Content,
    decimal RelevanceScore,
    DateTime CreatedAt
);