using Aruje.Application.DTOs.Sensors;
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

    public async Task<SensorResponse> CreateAsync(CreateSensorRequest request, CancellationToken cancellationToken = default)
    {
        var crop = await _cropRepository.GetByIdAsync(request.CropId);

        if (crop is null)
            throw new ArgumentException("Crop not found.");

        var sensor = new Sensor(
            request.Name,
            request.Type,
            request.SerialNumber,
            request.CropId
        );

        await _sensorRepository.AddAsync(sensor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SensorResponse(sensor.Id, sensor.Name, sensor.Type, sensor.SerialNumber, sensor.CropId, sensor.IsActive);
    }

    public async Task<IReadOnlyList<SensorResponse>> GetByCropIdAsync(Guid cropId)
    {
        var sensors = await _sensorRepository.GetByCropIdAsync(cropId);

        return sensors.Select(sensor =>
            new SensorResponse(sensor.Id, sensor.Name, sensor.Type, sensor.SerialNumber, sensor.CropId, sensor.IsActive)
        ).ToList();
    }
}