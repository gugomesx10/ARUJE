using Aruje.Application.DTOs.Sensors;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Domain.Entities;

namespace Aruje.Application.Services;

public class SensorService
{
    private readonly ISensorRepository _sensorRepository;
    private readonly ICropRepository _cropRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SensorService(
        ISensorRepository sensorRepository,
        ICropRepository cropRepository,
        IUnitOfWork unitOfWork)
    {
        _sensorRepository = sensorRepository;
        _cropRepository = cropRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SensorResponse>> GetAllAsync()
    {
        var sensors = await _sensorRepository.GetAllAsync();

        return sensors.Select(ToResponse).ToList();
    }

    public async Task<SensorResponse> GetByIdAsync(Guid id)
    {
        var sensor = await _sensorRepository.GetByIdAsync(id);

        if (sensor is null || !sensor.IsActive)
            throw new NotFoundException("Sensor not found.");

        return ToResponse(sensor);
    }

    public async Task<IReadOnlyList<SensorResponse>> GetByCropIdAsync(Guid cropId)
    {
        var crop = await _cropRepository.GetByIdAsync(cropId);

        if (crop is null || !crop.IsActive)
            throw new NotFoundException("Crop not found.");

        var sensors = await _sensorRepository.GetByCropIdAsync(cropId);

        return sensors.Select(ToResponse).ToList();
    }

    public async Task<SensorResponse> CreateAsync(
        CreateSensorRequest request,
        CancellationToken cancellationToken = default)
    {
        var crop = await _cropRepository.GetByIdAsync(request.CropId);

        if (crop is null || !crop.IsActive)
            throw new NotFoundException("Crop not found.");

        var sensor = new Sensor(
            request.Name,
            request.Type,
            request.SerialNumber,
            request.CropId
        );

        await _sensorRepository.AddAsync(sensor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(sensor);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateSensorRequest request,
        CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.GetByIdAsync(id);

        if (sensor is null || !sensor.IsActive)
            throw new NotFoundException("Sensor not found.");

        sensor.Update(
            request.Name,
            request.Type,
            request.SerialNumber
        );

        await _sensorRepository.UpdateAsync(sensor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.GetByIdAsync(id);

        if (sensor is null || !sensor.IsActive)
            throw new NotFoundException("Sensor not found.");

        await _sensorRepository.DeleteAsync(sensor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static SensorResponse ToResponse(Sensor sensor)
    {
        return new SensorResponse(
            sensor.Id,
            sensor.Name,
            sensor.Type,
            sensor.SerialNumber,
            sensor.CropId,
            sensor.IsActive
        );
    }
}