using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Repositories;

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    public AlertRepository(ArujeDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Alert>> GetBySensorReadingIdAsync(Guid sensorReadingId)
    {
        return await DbSet
            .Where(alert =>
                alert.IsActive &&
                alert.SensorReadingId == sensorReadingId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Alert>> GetByStatusAsync(AlertStatus status)
    {
        return await DbSet
            .Where(alert =>
                alert.IsActive &&
                alert.Status == status)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Alert>> GetBySeverityAsync(AlertSeverity severity)
    {
        return await DbSet
            .Where(alert =>
                alert.IsActive &&
                alert.Severity == severity)
            .ToListAsync();
    }
}