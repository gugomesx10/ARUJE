using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Repositories;

public class AiAnalysisRepository : Repository<AiAnalysis>, IAiAnalysisRepository
{
    public AiAnalysisRepository(ArujeDbContext context) : base(context)
    {
    }

    public async Task<AiAnalysis?> GetByAlertIdAsync(Guid alertId)
    {
        return await DbSet
            .FirstOrDefaultAsync(analysis =>
                analysis.IsActive &&
                analysis.AlertId == alertId);
    }
}