using Aruje.Application.DTOs.Rag;

namespace Aruje.Application.Interfaces.Services;

public interface IRagContextBuilder
{
    Task<RagContext> BuildAsync(
        string question,
        int maxItems,
        CancellationToken cancellationToken = default);
}