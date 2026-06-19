using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Aruje.Tests.Services;

public class AiAnalysisServiceTests
{
    private readonly Mock<IAiAnalysisRepository> _aiAnalysisRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AiAnalysisService _aiAnalysisService;

    public AiAnalysisServiceTests()
    {
        _aiAnalysisRepositoryMock = new Mock<IAiAnalysisRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _aiAnalysisService = new AiAnalysisService(
            _aiAnalysisRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task GenerateAnalysisAsync_ShouldGenerateAnalysis_WhenAlertAndReadingAreValid()
    {
        var reading = new SensorReading(
            Guid.NewGuid(),
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
            reading.Id
        );

        var result = await _aiAnalysisService.GenerateAnalysisAsync(alert, reading);

        result.Should().NotBeNull();
        result.AlertId.Should().Be(alert.Id);
        result.RiskLevel.Should().Be(alert.Severity.ToString());
        result.Reason.Should().Be(alert.Description);
        result.Recommendation.Should().NotBeNullOrWhiteSpace();
        result.Provider.Should().Be("RuleBased-Mock");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAnalyses_WhenAnalysesExist()
    {
        var analyses = new List<AiAnalysis>
        {
            new(
                Guid.NewGuid(),
                "High",
                "Baixa umidade do solo.",
                "Verificar irrigação.",
                "RuleBased-Mock"
            ),
            new(
                Guid.NewGuid(),
                "Medium",
                "Temperatura elevada.",
                "Monitorar temperatura.",
                "RuleBased-Mock"
            )
        };

        _aiAnalysisRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(analyses);

        var result = await _aiAnalysisService.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].RiskLevel.Should().Be("High");
        result[1].RiskLevel.Should().Be("Medium");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAnalysis_WhenAnalysisExists()
    {
        var analysisId = Guid.NewGuid();

        var analysis = new AiAnalysis(
            Guid.NewGuid(),
            "High",
            "Baixa umidade do solo.",
            "Verificar irrigação.",
            "RuleBased-Mock"
        );

        typeof(AiAnalysis)
            .BaseType!
            .GetProperty("Id")!
            .SetValue(analysis, analysisId);

        _aiAnalysisRepositoryMock
            .Setup(repository => repository.GetByIdAsync(analysisId))
            .ReturnsAsync(analysis);

        var result = await _aiAnalysisService.GetByIdAsync(analysisId);

        result.Should().NotBeNull();
        result.Id.Should().Be(analysisId);
        result.RiskLevel.Should().Be(analysis.RiskLevel);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenAnalysisDoesNotExist()
    {
        var analysisId = Guid.NewGuid();

        _aiAnalysisRepositoryMock
            .Setup(repository => repository.GetByIdAsync(analysisId))
            .ReturnsAsync((AiAnalysis?)null);

        var action = async () => await _aiAnalysisService.GetByIdAsync(analysisId);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("AI analysis not found.");
    }

    [Fact]
    public async Task GetByAlertIdAsync_ShouldReturnAnalysis_WhenAnalysisExistsForAlert()
    {
        var alertId = Guid.NewGuid();

        var analysis = new AiAnalysis(
            alertId,
            "High",
            "Baixa umidade do solo.",
            "Verificar irrigação.",
            "RuleBased-Mock"
        );

        _aiAnalysisRepositoryMock
            .Setup(repository => repository.GetByAlertIdAsync(alertId))
            .ReturnsAsync(analysis);

        var result = await _aiAnalysisService.GetByAlertIdAsync(alertId);

        result.Should().NotBeNull();
        result.AlertId.Should().Be(alertId);
        result.RiskLevel.Should().Be("High");
    }

    [Fact]
    public async Task GetByAlertIdAsync_ShouldThrowNotFoundException_WhenAnalysisDoesNotExistForAlert()
    {
        var alertId = Guid.NewGuid();

        _aiAnalysisRepositoryMock
            .Setup(repository => repository.GetByAlertIdAsync(alertId))
            .ReturnsAsync((AiAnalysis?)null);

        var action = async () => await _aiAnalysisService.GetByAlertIdAsync(alertId);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("AI analysis not found for this alert.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAnalysis_WhenAnalysisExists()
    {
        var analysisId = Guid.NewGuid();

        var analysis = new AiAnalysis(
            Guid.NewGuid(),
            "High",
            "Baixa umidade do solo.",
            "Verificar irrigação.",
            "RuleBased-Mock"
        );

        _aiAnalysisRepositoryMock
            .Setup(repository => repository.GetByIdAsync(analysisId))
            .ReturnsAsync(analysis);

        _aiAnalysisRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<AiAnalysis>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _aiAnalysisService.DeleteAsync(analysisId);

        _aiAnalysisRepositoryMock.Verify(
            repository => repository.DeleteAsync(analysis),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}