using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Repositories;

public interface ISensorRepository
{
    Task<Sensor?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Sensor>> GetAllAsync();
    Task<IReadOnlyList<Sensor>> GetByCropIdAsync(Guid cropId);

    Task AddAsync(Sensor sensor);
    Task UpdateAsync(Sensor sensor);
    Task DeleteAsync(Sensor sensor);
}