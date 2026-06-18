using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Services;

public interface IAlertService
{
    Task<Alert?> GenerateAlertFromReadingAsync(
        SensorReading reading,
        CancellationToken cancellationToken = default);
}