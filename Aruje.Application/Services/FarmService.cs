using Aruje.Application.DTOs.Farms;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;

namespace Aruje.Application.Services;

public class FarmService
{
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FarmService(IFarmRepository farmRepository, IUnitOfWork unitOfWork)
    {
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FarmResponse> CreateAsync(CreateFarmRequest request, CancellationToken cancellationToken = default)
    {
        var farm = new Farm(
            request.Name,
            request.OwnerName,
            request.Location,
            request.TotalAreaHectares
        );

        await _farmRepository.AddAsync(farm);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

    public async Task<IReadOnlyList<FarmResponse>> GetAllAsync()
    {
        var farms = await _farmRepository.GetAllAsync();

        return farms.Select(farm => new FarmResponse(
            farm.Id,
            farm.Name,
            farm.OwnerName,
            farm.Location,
            farm.TotalAreaHectares,
            farm.IsActive,
            farm.CreatedAt
        )).ToList();
    }
}