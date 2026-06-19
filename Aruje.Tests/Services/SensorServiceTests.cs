using Aruje.Application.DTOs.Sensors;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Aruje.Tests.Services;

public class SensorServiceTests
{
    private readonly Mock<ISensorRepository> _sensorRepositoryMock;
    private readonly Mock<ICropRepository> _cropRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SensorService _sensorService;

    public SensorServiceTests()
    {
        _sensorRepositoryMock = new Mock<ISensorRepository>();
        _cropRepositoryMock = new Mock<ICropRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sensorService = new SensorService(
            _sensorRepositoryMock.Object,
            _cropRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateSensor_WhenCropExists()
    {
        var cropId = Guid.NewGuid();

        var crop = new Crop(
            "Soja",
            CropType.Soybean,
            50,
            DateTime.UtcNow,
            Guid.NewGuid()
        );

        var request = new CreateSensorRequest(
            "Sensor de temperatura",
            SensorType.Temperature,
            "TEMP-001",
            cropId
        );

        Sensor? capturedSensor = null;

        _cropRepositoryMock
            .Setup(repository => repository.GetByIdAsync(cropId))
            .ReturnsAsync(crop);

        _sensorRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Sensor>()))
            .Callback<Sensor>(sensor => capturedSensor = sensor)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sensorService.CreateAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Type.Should().Be(request.Type);
        result.SerialNumber.Should().Be(request.SerialNumber);
        result.CropId.Should().Be(request.CropId);
        result.IsActive.Should().BeTrue();

        capturedSensor.Should().NotBeNull();

        _sensorRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Sensor>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenCropDoesNotExist()
    {
        var cropId = Guid.NewGuid();

        var request = new CreateSensorRequest(
            "Sensor de temperatura",
            SensorType.Temperature,
            "TEMP-001",
            cropId
        );

        _cropRepositoryMock
            .Setup(repository => repository.GetByIdAsync(cropId))
            .ReturnsAsync((Crop?)null);

        var action = async () => await _sensorService.CreateAsync(request);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Crop not found.");

        _sensorRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Sensor>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSensors_WhenSensorsExist()
    {
        var cropId = Guid.NewGuid();

        var sensors = new List<Sensor>
        {
            new("Sensor 1", SensorType.Temperature, "TEMP-001", cropId),
            new("Sensor 2", SensorType.SoilMoisture, "SOIL-001", cropId)
        };

        _sensorRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(sensors);

        var result = await _sensorService.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Sensor 1");
        result[1].Name.Should().Be("Sensor 2");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSensor_WhenSensorExists()
    {
        var sensorId = Guid.NewGuid();
        var cropId = Guid.NewGuid();

        var sensor = new Sensor(
            "Sensor de temperatura",
            SensorType.Temperature,
            "TEMP-001",
            cropId
        );

        typeof(Sensor)
            .BaseType!
            .GetProperty("Id")!
            .SetValue(sensor, sensorId);

        _sensorRepositoryMock
            .Setup(repository => repository.GetByIdAsync(sensorId))
            .ReturnsAsync(sensor);

        var result = await _sensorService.GetByIdAsync(sensorId);

        result.Should().NotBeNull();
        result.Id.Should().Be(sensorId);
        result.Name.Should().Be(sensor.Name);
        result.SerialNumber.Should().Be(sensor.SerialNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenSensorDoesNotExist()
    {
        var sensorId = Guid.NewGuid();

        _sensorRepositoryMock
            .Setup(repository => repository.GetByIdAsync(sensorId))
            .ReturnsAsync((Sensor?)null);

        var action = async () => await _sensorService.GetByIdAsync(sensorId);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Sensor not found.");
    }

    [Fact]
    public async Task GetByCropIdAsync_ShouldReturnSensors_WhenCropExists()
    {
        var cropId = Guid.NewGuid();

        var crop = new Crop(
            "Soja",
            CropType.Soybean,
            50,
            DateTime.UtcNow,
            Guid.NewGuid()
        );

        var sensors = new List<Sensor>
        {
            new("Sensor 1", SensorType.Temperature, "TEMP-001", cropId),
            new("Sensor 2", SensorType.SoilMoisture, "SOIL-001", cropId)
        };

        _cropRepositoryMock
            .Setup(repository => repository.GetByIdAsync(cropId))
            .ReturnsAsync(crop);

        _sensorRepositoryMock
            .Setup(repository => repository.GetByCropIdAsync(cropId))
            .ReturnsAsync(sensors);

        var result = await _sensorService.GetByCropIdAsync(cropId);

        result.Should().HaveCount(2);
        result.All(sensor => sensor.CropId == cropId).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSensor_WhenSensorExists()
    {
        var sensorId = Guid.NewGuid();
        var cropId = Guid.NewGuid();

        var sensor = new Sensor(
            "Sensor antigo",
            SensorType.Temperature,
            "TEMP-001",
            cropId
        );

        var request = new UpdateSensorRequest(
            "Sensor atualizado",
            SensorType.SoilMoisture,
            "SOIL-999"
        );

        _sensorRepositoryMock
            .Setup(repository => repository.GetByIdAsync(sensorId))
            .ReturnsAsync(sensor);

        _sensorRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Sensor>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _sensorService.UpdateAsync(sensorId, request);

        sensor.Name.Should().Be(request.Name);
        sensor.Type.Should().Be(request.Type);
        sensor.SerialNumber.Should().Be(request.SerialNumber);

        _sensorRepositoryMock.Verify(
            repository => repository.UpdateAsync(sensor),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteSensor_WhenSensorExists()
    {
        var sensorId = Guid.NewGuid();
        var cropId = Guid.NewGuid();

        var sensor = new Sensor(
            "Sensor de temperatura",
            SensorType.Temperature,
            "TEMP-001",
            cropId
        );

        _sensorRepositoryMock
            .Setup(repository => repository.GetByIdAsync(sensorId))
            .ReturnsAsync(sensor);

        _sensorRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<Sensor>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _sensorService.DeleteAsync(sensorId);

        _sensorRepositoryMock.Verify(
            repository => repository.DeleteAsync(sensor),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}