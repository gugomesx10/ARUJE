using Aruje.Application.DTOs.SensorReadings;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Domain.Entities;

namespace Aruje.Application.Services;

public class SensorReadingService : IIoTIngestionService
{
    private readonly ISensorRepository _sensorRepository;
    private readonly ISensorReadingRepository _sensorReadingRepository;
    private readonly IAlertService _alertService;
    private readonly IAlertRepository _alertRepository;
    private readonly IAiAnalysisService _aiAnalysisService;
    private readonly IAiAnalysisRepository _aiAnalysisRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SensorReadingService(
        ISensorRepository sensorRepository,
        ISensorReadingRepository sensorReadingRepository,
        IAlertService alertService,
        IAlertRepository alertRepository,
        IAiAnalysisService aiAnalysisService,
        IAiAnalysisRepository aiAnalysisRepository,
        IUnitOfWork unitOfWork)
    {
        _sensorRepository = sensorRepository;
        _sensorReadingRepository = sensorReadingRepository;
        _alertService = alertService;
        _alertRepository = alertRepository;
        _aiAnalysisService = aiAnalysisService;
        _aiAnalysisRepository = aiAnalysisRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SensorReading> RegisterReadingAsync(
        Guid sensorId,
        double? temperature,
        double? airHumidity,
        double? soilMoisture,
        double? luminosity,
        DateTime readingDate,
        CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.GetByIdAsync(sensorId);

        if (sensor is null)
            throw new ArgumentException("Sensor not found.");

        var reading = new SensorReading(
            sensorId,
            temperature,
            airHumidity,
            soilMoisture,
            luminosity,
            readingDate
        );

        await _sensorReadingRepository.AddAsync(reading);

        var alert = await _alertService.GenerateAlertFromReadingAsync(
            reading,
            cancellationToken
        );

        if (alert is not null)
        {
            await _alertRepository.AddAsync(alert);

            var aiAnalysis = await _aiAnalysisService.GenerateAnalysisAsync(
                alert,
                reading,
                cancellationToken
            );

            await _aiAnalysisRepository.AddAsync(aiAnalysis);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return reading;
    }

    public async Task<SensorReadingResponse> CreateAsync(
        CreateSensorReadingRequest request,
        CancellationToken cancellationToken = default)
    {
        var reading = await RegisterReadingAsync(
            request.SensorId,
            request.Temperature,
            request.AirHumidity,
            request.SoilMoisture,
            request.Luminosity,
            request.ReadingDate,
            cancellationToken
        );

        return new SensorReadingResponse(
            reading.Id,
            reading.SensorId,
            reading.Temperature,
            reading.AirHumidity,
            reading.SoilMoisture,
            reading.Luminosity,
            reading.ReadingDate
        );
    }
}