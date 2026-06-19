using Aruje.Application.DTOs.Crops;
using Aruje.Application.Exceptions;
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

    public async Task<IReadOnlyList<CropResponse>> GetAllAsync()
    {
        var crops = await _cropRepository.GetAllAsync();

        return crops.Select(ToResponse).ToList();
    }

    public async Task<CropResponse> GetByIdAsync(Guid id)
    {
        var crop = await _cropRepository.GetByIdAsync(id);

        if (crop is null || !crop.IsActive)
            throw new NotFoundException("Crop not found.");

        return ToResponse(crop);
    }

    public async Task<IReadOnlyList<CropResponse>> GetByFarmIdAsync(Guid farmId)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId);

        if (farm is null || !farm.IsActive)
            throw new NotFoundException("Farm not found.");

        var crops = await _cropRepository.GetByFarmIdAsync(farmId);

        return crops.Select(ToResponse).ToList();
    }

    public async Task<CropResponse> CreateAsync(
        CreateCropRequest request,
        CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(request.FarmId);

        if (farm is null || !farm.IsActive)
            throw new NotFoundException("Farm not found.");

        var crop = new Crop(
            request.Name,
            request.Type,
            request.AreaHectares,
            request.PlantingDate,
            request.FarmId
        );

        await _cropRepository.AddAsync(crop);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(crop);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateCropRequest request,
        CancellationToken cancellationToken = default)
    {
        var crop = await _cropRepository.GetByIdAsync(id);

        if (crop is null || !crop.IsActive)
            throw new NotFoundException("Crop not found.");

        crop.Update(
            request.Name,
            request.Type,
            request.AreaHectares,
            request.PlantingDate
        );

        await _cropRepository.UpdateAsync(crop);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var crop = await _cropRepository.GetByIdAsync(id);

        if (crop is null || !crop.IsActive)
            throw new NotFoundException("Crop not found.");

        await _cropRepository.DeleteAsync(crop);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static CropResponse ToResponse(Crop crop)
    {
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
}