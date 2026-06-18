using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Repositories;

public interface ICropRepository
{
    Task<Crop?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Crop>> GetAllAsync();
    Task<IReadOnlyList<Crop>> GetByFarmIdAsync(Guid farmId);
    Task AddAsync(Crop crop);
    Task UpdateAsync(Crop crop);
    Task DeleteAsync(Crop crop);
}