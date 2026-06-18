using Aruje.Domain.Entities;
using Aruje.Domain.Enums;

namespace Aruje.Application.Interfaces.Repositories;

public interface IAlertRepository
{
    Task<Alert?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<Alert>> GetAllAsync();

    Task<IReadOnlyList<Alert>> GetBySensorReadingIdAsync(Guid sensorReadingId);

    Task<IReadOnlyList<Alert>> GetByStatusAsync(AlertStatus status);

    Task<IReadOnlyList<Alert>> GetBySeverityAsync(AlertSeverity severity);

    Task AddAsync(Alert alert);

    Task UpdateAsync(Alert alert);

    Task DeleteAsync(Alert alert);
}