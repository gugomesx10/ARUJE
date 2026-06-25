using Aruje.Application.DTOs.Rag;

namespace Aruje.Application.Interfaces.Services;

public interface IRagLlmProvider
{
    Task<RagLlmResponse> GenerateAsync(
        string prompt,
        RagContext context,
        CancellationToken cancellationToken = default);
}