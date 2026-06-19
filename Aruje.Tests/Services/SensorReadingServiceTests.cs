using Aruje.Application.DTOs.SensorReadings;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Application.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;
using FluentAssertions;
using Moq;
using AppValidationException = Aruje.Application.Exceptions.ValidationException;

namespace Aruje.Tests.Services;

public class SensorReadingServiceTests
{
    private readonly Mock<ISensorRepository> _sensorRepositoryMock;
    private readonly Mock<ISensorReadingRepository> _sensorReadingRepositoryMock;
    private readonly Mock<IAlertService> _alertServiceMock;
    private readonly Mock<IAlertRepository> _alertRepositoryMock;
    private readonly Mock<IAiAnalysisService> _aiAnalysisServiceMock;
    private readonly Mock<IAiAnalysisRepository> _aiAnalysisRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SensorReadingService _sensorReadingService;

    public SensorReadingServiceTests()
    {
        _sensorRepositoryMock = new Mock<ISensorRepository>();
        _sensorReadingRepositoryMock = new Mock<ISensorReadingRepository>();
        _alertServiceMock = new Mock<IAlertService>();
        _alertRepositoryMock = new Mock<IAlertRepository>();
        _aiAnalysisServiceMock = new Mock<IAiAnalysisService>();
        _aiAnalysisRepositoryMock = new Mock<IAiAnalysisRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sensorReadingService = new SensorReadingService(
            _sensorRepositoryMock.Object,
            _sensorReadingRepositoryMock.Object,
            _alertServiceMock.Object,
            _alertRepositoryMock.Object,
            _aiAnalysisServiceMock.Object,
            _aiAnalysisRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateReading_WhenSensorExistsAndNoAlertIsGenerated()
    {
        var sensorId = Guid.NewGuid();

        var sensor = new Sensor(
            "Sensor de temperatura",
            SensorType.Temperature,
            "TEMP-001",
            Guid.NewGuid()
        );

        var request = new CreateSensorReadingRequest(
            sensorId,
            25,
            60,
            50,
            700,
            DateTime.UtcNow
        );

        SensorReading? capturedReading = null;

        _sensorRepositoryMock
            .Setup(repository => repository.GetByIdAsync(sensorId))
            .ReturnsAsync(sensor);

        _sensorReadingRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<SensorReading>()))
            .Callback<SensorReading>(reading => capturedReading = reading)
            .Returns(Task.CompletedTask);

        _alertServiceMock
            .Setup(service => service.GenerateAlertFromReadingAsync(
                It.IsAny<SensorReading>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert?)null);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sensorReadingService.CreateAsync(request);

        result.Should().NotBeNull();
        result.SensorId.Should().Be(sensorId);
        result.Temperature.Should().Be(request.Temperature);
        result.AirHumidity.Should().Be(request.AirHumidity);
        result.SoilMoisture.Should().Be(request.SoilMoisture);
        result.Luminosity.Should().Be(request.Luminosity);

        capturedReading.Should().NotBeNull();

        _sensorReadingRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<SensorReading>()),
            Times.Once
        );

        _alertRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Alert>()),
            Times.Never
        );

        _aiAnalysisRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<AiAnalysis>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateReadingAlertAndAiAnalysis_WhenCriticalReadingIsSent()
    {
        var sensorId = Guid.NewGuid();

        var sensor = new Sensor(
            "Sensor de solo",
            SensorType.SoilMoisture,
            "SOIL-001",
            Guid.NewGuid()
        );

        var request = new CreateSensorReadingRequest(
            sensorId,
            39,
            20,
            15,
            800,
            DateTime.UtcNow
        );

        var alert = new Alert(
            "Risco de estresse hídrico",
            "Temperatura elevada combinada com baixa umidade do solo.",
            AlertSeverity.High,
            Guid.NewGuid()
        );

        var aiAnalysis = new AiAnalysis(
            alert.Id,
            "High",
            alert.Description,
            "Verificar a plantação e avaliar necessidade de irrigação.",
            "RuleBased-Mock"
        );

        _sensorRepositoryMock
            .Setup(repository => repository.GetByIdAsync(sensorId))
            .ReturnsAsync(sensor);

        _sensorReadingRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<SensorReading>()))
            .Returns(Task.CompletedTask);

        _alertServiceMock
            .Setup(service => service.GenerateAlertFromReadingAsync(
                It.IsAny<SensorReading>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alert);

        _alertRepositoryMock
            .Setup(repository => repository.AddAsync(alert))
            .Returns(Task.CompletedTask);

        _aiAnalysisServiceMock
            .Setup(service => service.GenerateAnalysisAsync(
                alert,
                It.IsAny<SensorReading>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiAnalysis);

        _aiAnalysisRepositoryMock
            .Setup(repository => repository.AddAsync(aiAnalysis))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sensorReadingService.CreateAsync(request);

        result.Should().NotBeNull();
        result.SensorId.Should().Be(sensorId);
        result.Temperature.Should().Be(39);
        result.SoilMoisture.Should().Be(15);

        _sensorReadingRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<SensorReading>()),
            Times.Once
        );

        _alertRepositoryMock.Verify(
            repository => repository.AddAsync(alert),
            Times.Once
        );

        _aiAnalysisRepositoryMock.Verify(
            repository => repository.AddAsync(aiAnalysis),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenSensorDoesNotExist()
    {
        var sensorId = Guid.NewGuid();

        var request = new CreateSensorReadingRequest(
            sensorId,
            25,
            60,
            50,
            700,
            DateTime.UtcNow
        );

        _sensorRepositoryMock
            .Setup(repository => repository.GetByIdAsync(sensorId))
            .ReturnsAsync((Sensor?)null);

        var action = async () => await _sensorReadingService.CreateAsync(request);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Sensor not found.");

        _sensorReadingRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<SensorReading>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnReadings_WhenReadingsExist()
    {
        var sensorId = Guid.NewGuid();

        var readings = new List<SensorReading>
        {
            new(sensorId, 25, 60, 50, 700, DateTime.UtcNow),
            new(sensorId, 30, 55, 45, 750, DateTime.UtcNow)
        };

        _sensorReadingRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(readings);

        var result = await _sensorReadingService.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].SensorId.Should().Be(sensorId);
        result[1].SensorId.Should().Be(sensorId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReading_WhenReadingExists()
    {
        var readingId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();

        var reading = new SensorReading(
            sensorId,
            25,
            60,
            50,
            700,
            DateTime.UtcNow
        );

        typeof(SensorReading)
            .BaseType!
            .GetProperty("Id")!
            .SetValue(reading, readingId);

        _sensorReadingRepositoryMock
            .Setup(repository => repository.GetByIdAsync(readingId))
            .ReturnsAsync(reading);

        var result = await _sensorReadingService.GetByIdAsync(readingId);

        result.Should().NotBeNull();
        result.Id.Should().Be(readingId);
        result.SensorId.Should().Be(sensorId);
    }

    [Fact]
    public async Task GetBySensorIdAsync_ShouldReturnReadings_WhenSensorExists()
    {
        var sensorId = Guid.NewGuid();

        var sensor = new Sensor(
            "Sensor de temperatura",
            SensorType.Temperature,
            "TEMP-001",
            Guid.NewGuid()
        );

        var readings = new List<SensorReading>
        {
            new(sensorId, 25, 60, 50, 700, DateTime.UtcNow),
            new(sensorId, 26, 59, 49, 710, DateTime.UtcNow)
        };

        _sensorRepositoryMock
            .Setup(repository => repository.GetByIdAsync(sensorId))
            .ReturnsAsync(sensor);

        _sensorReadingRepositoryMock
            .Setup(repository => repository.GetBySensorIdAsync(sensorId))
            .ReturnsAsync(readings);

        var result = await _sensorReadingService.GetBySensorIdAsync(sensorId);

        result.Should().HaveCount(2);
        result.All(reading => reading.SensorId == sensorId).Should().BeTrue();
    }

    [Fact]
    public async Task GetLatestBySensorIdAsync_ShouldThrowValidationException_WhenQuantityIsInvalid()
    {
        var sensorId = Guid.NewGuid();

        var action = async () => await _sensorReadingService.GetLatestBySensorIdAsync(
            sensorId,
            0
        );

        await action.Should().ThrowAsync<AppValidationException>()
            .WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteReading_WhenReadingExists()
    {
        var readingId = Guid.NewGuid();

        var reading = new SensorReading(
            Guid.NewGuid(),
            25,
            60,
            50,
            700,
            DateTime.UtcNow
        );

        _sensorReadingRepositoryMock
            .Setup(repository => repository.GetByIdAsync(readingId))
            .ReturnsAsync(reading);

        _sensorReadingRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<SensorReading>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _sensorReadingService.DeleteAsync(readingId);

        _sensorReadingRepositoryMock.Verify(
            repository => repository.DeleteAsync(reading),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}