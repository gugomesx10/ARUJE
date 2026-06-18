using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Repositories;

public class SensorRepository : Repository<Sensor>, ISensorRepository
{
    public SensorRepository(ArujeDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Sensor>> GetByCropIdAsync(Guid cropId)
    {
        return await DbSet
            .Where(x => x.CropId == cropId && x.IsActive)
            .ToListAsync();
    }
}