using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Aruje.Tests.Services;

public class AlertServiceTests
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AlertService _alertService;

    public AlertServiceTests()
    {
        _alertRepositoryMock = new Mock<IAlertRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _alertService = new AlertService(
            _alertRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task GenerateAlertFromReadingAsync_ShouldReturnHighAlert_WhenTemperatureAndSoilMoistureAreCritical()
    {
        var reading = new SensorReading(
            Guid.NewGuid(),
            39,
            20,
            15,
            800,
            DateTime.UtcNow
        );

        var result = await _alertService.GenerateAlertFromReadingAsync(reading);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Risco de estresse hídrico");
        result.Severity.Should().Be(AlertSeverity.High);
        result.SensorReadingId.Should().Be(reading.Id);
    }

    [Fact]
    public async Task GenerateAlertFromReadingAsync_ShouldReturnMediumAlert_WhenTemperatureIsHigh()
    {
        var reading = new SensorReading(
            Guid.NewGuid(),
            36,
            60,
            50,
            800,
            DateTime.UtcNow
        );

        var result = await _alertService.GenerateAlertFromReadingAsync(reading);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Temperatura elevada");
        result.Severity.Should().Be(AlertSeverity.Medium);
    }

    [Fact]
    public async Task GenerateAlertFromReadingAsync_ShouldReturnHighAlert_WhenSoilMoistureIsLow()
    {
        var reading = new SensorReading(
            Guid.NewGuid(),
            25,
            60,
            15,
            800,
            DateTime.UtcNow
        );

        var result = await _alertService.GenerateAlertFromReadingAsync(reading);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Baixa umidade do solo");
        result.Severity.Should().Be(AlertSeverity.High);
    }

    [Fact]
    public async Task GenerateAlertFromReadingAsync_ShouldReturnNull_WhenReadingIsNormal()
    {
        var reading = new SensorReading(
            Guid.NewGuid(),
            25,
            60,
            50,
            800,
            DateTime.UtcNow
        );

        var result = await _alertService.GenerateAlertFromReadingAsync(reading);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAlerts_WhenAlertsExist()
    {
        var alerts = new List<Alert>
        {
            new(
                "Temperatura elevada",
                "Temperatura acima do recomendado.",
                AlertSeverity.Medium,
                Guid.NewGuid()
            ),
            new(
                "Baixa umidade do solo",
                "Umidade abaixo do recomendado.",
                AlertSeverity.High,
                Guid.NewGuid()
            )
        };

        _alertRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(alerts);

        var result = await _alertService.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Temperatura elevada");
        result[1].Title.Should().Be("Baixa umidade do solo");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAlert_WhenAlertExists()
    {
        var alertId = Guid.NewGuid();

        var alert = new Alert(
            "Temperatura elevada",
            "Temperatura acima do recomendado.",
            AlertSeverity.Medium,
            Guid.NewGuid()
        );

        typeof(Alert)
            .BaseType!
            .GetProperty("Id")!
            .SetValue(alert, alertId);

        _alertRepositoryMock
            .Setup(repository => repository.GetByIdAsync(alertId))
            .ReturnsAsync(alert);

        var result = await _alertService.GetByIdAsync(alertId);

        result.Should().NotBeNull();
        result.Id.Should().Be(alertId);
        result.Title.Should().Be(alert.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenAlertDoesNotExist()
    {
        var alertId = Guid.NewGuid();

        _alertRepositoryMock
            .Setup(repository => repository.GetByIdAsync(alertId))
            .ReturnsAsync((Alert?)null);

        var action = async () => await _alertService.GetByIdAsync(alertId);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Alert not found.");
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnAlerts_WhenAlertsExistWithStatus()
    {
        var alerts = new List<Alert>
        {
            new(
                "Temperatura elevada",
                "Temperatura acima do recomendado.",
                AlertSeverity.Medium,
                Guid.NewGuid()
            )
        };

        _alertRepositoryMock
            .Setup(repository => repository.GetByStatusAsync(AlertStatus.Open))
            .ReturnsAsync(alerts);

        var result = await _alertService.GetByStatusAsync(AlertStatus.Open);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(AlertStatus.Open);
    }

    [Fact]
    public async Task GetBySeverityAsync_ShouldReturnAlerts_WhenAlertsExistWithSeverity()
    {
        var alerts = new List<Alert>
        {
            new(
                "Baixa umidade do solo",
                "Umidade abaixo do recomendado.",
                AlertSeverity.High,
                Guid.NewGuid()
            )
        };

        _alertRepositoryMock
            .Setup(repository => repository.GetBySeverityAsync(AlertSeverity.High))
            .ReturnsAsync(alerts);

        var result = await _alertService.GetBySeverityAsync(AlertSeverity.High);

        result.Should().HaveCount(1);
        result[0].Severity.Should().Be(AlertSeverity.High);
    }

    [Fact]
    public async Task StartProcessingAsync_ShouldChangeStatusToInProgress_WhenAlertExists()
    {
        var alertId = Guid.NewGuid();

        var alert = new Alert(
            "Temperatura elevada",
            "Temperatura acima do recomendado.",
            AlertSeverity.Medium,
            Guid.NewGuid()
        );

        _alertRepositoryMock
            .Setup(repository => repository.GetByIdAsync(alertId))
            .ReturnsAsync(alert);

        _alertRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Alert>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _alertService.StartProcessingAsync(alertId);

        alert.Status.Should().Be(AlertStatus.InProgress);

        _alertRepositoryMock.Verify(
            repository => repository.UpdateAsync(alert),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ResolveAsync_ShouldChangeStatusToResolved_WhenAlertExists()
    {
        var alertId = Guid.NewGuid();

        var alert = new Alert(
            "Temperatura elevada",
            "Temperatura acima do recomendado.",
            AlertSeverity.Medium,
            Guid.NewGuid()
        );

        _alertRepositoryMock
            .Setup(repository => repository.GetByIdAsync(alertId))
            .ReturnsAsync(alert);

        _alertRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Alert>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _alertService.ResolveAsync(alertId);

        alert.Status.Should().Be(AlertStatus.Resolved);

        _alertRepositoryMock.Verify(
            repository => repository.UpdateAsync(alert),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CloseAsync_ShouldChangeStatusToClosed_WhenAlertExists()
    {
        var alertId = Guid.NewGuid();

        var alert = new Alert(
            "Temperatura elevada",
            "Temperatura acima do recomendado.",
            AlertSeverity.Medium,
            Guid.NewGuid()
        );

        _alertRepositoryMock
            .Setup(repository => repository.GetByIdAsync(alertId))
            .ReturnsAsync(alert);

        _alertRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Alert>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _alertService.CloseAsync(alertId);

        alert.Status.Should().Be(AlertStatus.Closed);

        _alertRepositoryMock.Verify(
            repository => repository.UpdateAsync(alert),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAlert_WhenAlertExists()
    {
        var alertId = Guid.NewGuid();

        var alert = new Alert(
            "Temperatura elevada",
            "Temperatura acima do recomendado.",
            AlertSeverity.Medium,
            Guid.NewGuid()
        );

        _alertRepositoryMock
            .Setup(repository => repository.GetByIdAsync(alertId))
            .ReturnsAsync(alert);

        _alertRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<Alert>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _alertService.DeleteAsync(alertId);

        _alertRepositoryMock.Verify(
            repository => repository.DeleteAsync(alert),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}