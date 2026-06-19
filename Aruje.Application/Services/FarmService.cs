using Aruje.Application.DTOs.Farms;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;

namespace Aruje.Application.Services;

public class FarmService
{
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FarmService(
        IFarmRepository farmRepository,
        IUnitOfWork unitOfWork)
    {
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<FarmResponse>> GetAllAsync()
    {
        var farms = await _farmRepository.GetAllAsync();

        return farms.Select(ToResponse).ToList();
    }

    public async Task<FarmResponse> GetByIdAsync(Guid id)
    {
        var farm = await _farmRepository.GetByIdAsync(id);

        if (farm is null || !farm.IsActive)
            throw new NotFoundException("Farm not found.");

        return ToResponse(farm);
    }

    public async Task<IReadOnlyList<FarmResponse>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Farm name filter is required.");

        var farms = await _farmRepository.SearchByNameAsync(name);

        return farms.Select(ToResponse).ToList();
    }

    public async Task<FarmResponse> CreateAsync(
        CreateFarmRequest request,
        CancellationToken cancellationToken = default)
    {
        var farm = new Farm(
            request.Name,
            request.OwnerName,
            request.Location,
            request.TotalAreaHectares
        );

        await _farmRepository.AddAsync(farm);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(farm);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateFarmRequest request,
        CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(id);

        if (farm is null || !farm.IsActive)
            throw new NotFoundException("Farm not found.");

        farm.Update(
            request.Name,
            request.OwnerName,
            request.Location,
            request.TotalAreaHectares
        );

        await _farmRepository.UpdateAsync(farm);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(id);

        if (farm is null || !farm.IsActive)
            throw new NotFoundException("Farm not found.");

        await _farmRepository.DeleteAsync(farm);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static FarmResponse ToResponse(Farm farm)
    {
        return new FarmResponse(
            farm.Id,
            farm.Name,
            farm.OwnerName,
            farm.Location,
            farm.TotalAreaHectares,
            farm.IsActive,
            farm.CreatedAt
        );
    }
}