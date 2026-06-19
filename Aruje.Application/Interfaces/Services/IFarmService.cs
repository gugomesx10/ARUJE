using Aruje.Application.DTOs.Farms;

namespace Aruje.Application.Interfaces.Services;

public interface IFarmService
{
    Task<IReadOnlyList<FarmResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<FarmResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FarmResponse>> SearchByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<FarmResponse> CreateAsync(
        CreateFarmRequest request,
        CancellationToken cancellationToken = default);

    Task<FarmResponse> UpdateAsync(
        Guid id,
        UpdateFarmRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}