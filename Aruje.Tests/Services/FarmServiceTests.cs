using Aruje.Application.DTOs.Farms;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Services;
using Aruje.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Aruje.Tests.Services;

public class FarmServiceTests
{
    private readonly Mock<IFarmRepository> _farmRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FarmService _farmService;

    public FarmServiceTests()
    {
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _farmService = new FarmService(
            _farmRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateFarm_WhenRequestIsValid()
    {
        var request = new CreateFarmRequest(
            "Fazenda Boa Esperança",
            "Gustavo Gomes",
            "São Paulo - SP",
            120.5
        );

        Farm? capturedFarm = null;

        _farmRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Farm>()))
            .Callback<Farm>(farm => capturedFarm = farm)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _farmService.CreateAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.OwnerName.Should().Be(request.OwnerName);
        result.Location.Should().Be(request.Location);
        result.TotalAreaHectares.Should().Be(request.TotalAreaHectares);
        result.IsActive.Should().BeTrue();

        capturedFarm.Should().NotBeNull();

        _farmRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Farm>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFarms_WhenFarmsExist()
    {
        var farms = new List<Farm>
        {
            new("Fazenda 1", "Gustavo", "São Paulo - SP", 100),
            new("Fazenda 2", "Gustavo", "Minas Gerais - MG", 200)
        };

        _farmRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(farms);

        var result = await _farmService.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Fazenda 1");
        result[1].Name.Should().Be("Fazenda 2");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFarm_WhenFarmExists()
    {
        var farmId = Guid.NewGuid();

        var farm = new Farm(
            "Fazenda Boa Esperança",
            "Gustavo Gomes",
            "São Paulo - SP",
            120.5
        );

        typeof(Farm)
            .BaseType!
            .GetProperty("Id")!
            .SetValue(farm, farmId);

        _farmRepositoryMock
            .Setup(repository => repository.GetByIdAsync(farmId))
            .ReturnsAsync(farm);

        var result = await _farmService.GetByIdAsync(farmId);

        result.Should().NotBeNull();
        result.Id.Should().Be(farmId);
        result.Name.Should().Be(farm.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenFarmDoesNotExist()
    {
        var farmId = Guid.NewGuid();

        _farmRepositoryMock
            .Setup(repository => repository.GetByIdAsync(farmId))
            .ReturnsAsync((Farm?)null);

        var action = async () => await _farmService.GetByIdAsync(farmId);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Farm not found.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFarm_WhenFarmExists()
    {
        var farmId = Guid.NewGuid();

        var farm = new Farm(
            "Fazenda Antiga",
            "Gustavo",
            "São Paulo - SP",
            100
        );

        var request = new UpdateFarmRequest(
            "Fazenda Atualizada",
            "Gustavo Gomes",
            "Campinas - SP",
            150
        );

        _farmRepositoryMock
            .Setup(repository => repository.GetByIdAsync(farmId))
            .ReturnsAsync(farm);

        _farmRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Farm>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _farmService.UpdateAsync(farmId, request);

        farm.Name.Should().Be(request.Name);
        farm.OwnerName.Should().Be(request.OwnerName);
        farm.Location.Should().Be(request.Location);
        farm.TotalAreaHectares.Should().Be(request.TotalAreaHectares);

        _farmRepositoryMock.Verify(
            repository => repository.UpdateAsync(farm),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeactivateFarm_WhenFarmExists()
    {
        var farmId = Guid.NewGuid();

        var farm = new Farm(
            "Fazenda Boa Esperança",
            "Gustavo Gomes",
            "São Paulo - SP",
            120.5
        );

        _farmRepositoryMock
            .Setup(repository => repository.GetByIdAsync(farmId))
            .ReturnsAsync(farm);

        _farmRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<Farm>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _farmService.DeleteAsync(farmId);

        _farmRepositoryMock.Verify(
            repository => repository.DeleteAsync(farm),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}