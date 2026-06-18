using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Repositories;

public class CropRepository : Repository<Crop>, ICropRepository
{
    public CropRepository(ArujeDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Crop>> GetByFarmIdAsync(Guid farmId)
    {
        return await DbSet
            .Where(x => x.FarmId == farmId && x.IsActive)
            .ToListAsync();
    }
}