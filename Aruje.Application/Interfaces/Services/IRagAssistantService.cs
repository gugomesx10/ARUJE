using Aruje.Application.DTOs.Rag;

namespace Aruje.Application.Interfaces.Services;

public interface IRagAssistantService
{
    Task<RagAskResponse> AskAsync(
        RagAskRequest request,
        CancellationToken cancellationToken = default);
}