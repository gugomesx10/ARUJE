using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Repositories;

public interface IAiAnalysisRepository
{
    Task<AiAnalysis?> GetByIdAsync(Guid id);

    Task<AiAnalysis?> GetByAlertIdAsync(Guid alertId);

    Task<IReadOnlyList<AiAnalysis>> GetAllAsync();

    Task AddAsync(AiAnalysis aiAnalysis);

    Task DeleteAsync(AiAnalysis aiAnalysis);
}