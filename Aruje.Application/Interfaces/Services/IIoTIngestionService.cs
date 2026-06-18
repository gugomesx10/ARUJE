using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Services;

public interface IIoTIngestionService
{
    Task<SensorReading> RegisterReadingAsync(
        Guid sensorId,
        double? temperature,
        double? airHumidity,
        double? soilMoisture,
        double? luminosity,
        DateTime readingDate,
        CancellationToken cancellationToken = default);
}