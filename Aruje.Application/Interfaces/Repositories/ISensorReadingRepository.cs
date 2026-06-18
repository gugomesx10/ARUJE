using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Repositories;

public interface ISensorReadingRepository
{
    Task<SensorReading?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<SensorReading>> GetAllAsync();

    Task<IReadOnlyList<SensorReading>> GetBySensorIdAsync(Guid sensorId);

    Task<IReadOnlyList<SensorReading>> GetLatestBySensorIdAsync(
        Guid sensorId,
        int quantity);

    Task AddAsync(SensorReading sensorReading);

    Task DeleteAsync(SensorReading sensorReading);
}