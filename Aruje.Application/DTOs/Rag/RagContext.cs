namespace Aruje.Application.DTOs.Rag;

public record RagContext(
    string Question,
    IReadOnlyList<RagContextItem> Items,
    DateTime GeneratedAt
);