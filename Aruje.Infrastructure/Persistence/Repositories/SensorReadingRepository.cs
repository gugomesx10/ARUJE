using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Repositories;

public class SensorReadingRepository : Repository<SensorReading>, ISensorReadingRepository
{
    public SensorReadingRepository(ArujeDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SensorReading>> GetBySensorIdAsync(Guid sensorId)
    {
        return await DbSet
            .Where(x => x.SensorId == sensorId && x.IsActive)
            .OrderByDescending(x => x.ReadingDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SensorReading>> GetLatestBySensorIdAsync(Guid sensorId, int quantity)
    {
        return await DbSet
            .Where(x => x.SensorId == sensorId && x.IsActive)
            .OrderByDescending(x => x.ReadingDate)
            .Take(quantity)
            .ToListAsync();
    }
}