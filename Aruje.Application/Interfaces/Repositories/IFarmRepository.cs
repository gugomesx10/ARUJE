using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Repositories;

public interface IFarmRepository
{
    Task<Farm?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<Farm>> GetAllAsync();

    Task<IReadOnlyList<Farm>> SearchByNameAsync(string name);

    Task AddAsync(Farm farm);

    Task UpdateAsync(Farm farm);

    Task DeleteAsync(Farm farm);
}