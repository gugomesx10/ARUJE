using Aruje.Application.DTOs.Crops;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;

namespace Aruje.Application.Services;

public class CropService
{
    private readonly ICropRepository _cropRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CropService(
        ICropRepository cropRepository,
        IFarmRepository farmRepository,
        IUnitOfWork unitOfWork)
    {
        _cropRepository = cropRepository;
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CropResponse> CreateAsync(
        CreateCropRequest request,
        CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(request.FarmId);

        if (farm is null)
            throw new ArgumentException("Farm not found.");

        var crop = new Crop(
            request.Name,
            request.Type,
            request.AreaHectares,
            request.PlantingDate,
            request.FarmId
        );

        await _cropRepository.AddAsync(crop);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CropResponse(
            crop.Id,
            crop.Name,
            crop.Type,
            crop.AreaHectares,
            crop.PlantingDate,
            crop.FarmId,
            crop.IsActive
        );
    }

    public async Task<IReadOnlyList<CropResponse>> GetByFarmIdAsync(Guid farmId)
    {
        var crops = await _cropRepository.GetByFarmIdAsync(farmId);

        return crops.Select(crop => new CropResponse(
            crop.Id,
            crop.Name,
            crop.Type,
            crop.AreaHectares,
            crop.PlantingDate,
            crop.FarmId,
            crop.IsActive
        )).ToList();
    }
}