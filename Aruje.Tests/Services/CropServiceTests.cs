using Aruje.Application.DTOs.Crops;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Aruje.Tests.Services;

public class CropServiceTests
{
    private readonly Mock<ICropRepository> _cropRepositoryMock;
    private readonly Mock<IFarmRepository> _farmRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CropService _cropService;

    public CropServiceTests()
    {
        _cropRepositoryMock = new Mock<ICropRepository>();
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _cropService = new CropService(
            _cropRepositoryMock.Object,
            _farmRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCrop_WhenFarmExists()
    {
        var farmId = Guid.NewGuid();

        var farm = new Farm(
            "Fazenda Boa Esperança",
            "Gustavo Gomes",
            "São Paulo - SP",
            120.5
        );

        var request = new CreateCropRequest(
            "Plantação de soja",
            CropType.Soybean,
            50,
            DateTime.UtcNow,
            farmId
        );

        Crop? capturedCrop = null;

        _farmRepositoryMock
            .Setup(repository => repository.GetByIdAsync(farmId))
            .ReturnsAsync(farm);

        _cropRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Crop>()))
            .Callback<Crop>(crop => capturedCrop = crop)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _cropService.CreateAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Type.Should().Be(request.Type);
        result.AreaHectares.Should().Be(request.AreaHectares);
        result.FarmId.Should().Be(request.FarmId);
        result.IsActive.Should().BeTrue();

        capturedCrop.Should().NotBeNull();

        _cropRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Crop>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenFarmDoesNotExist()
    {
        var farmId = Guid.NewGuid();

        var request = new CreateCropRequest(
            "Plantação de soja",
            CropType.Soybean,
            50,
            DateTime.UtcNow,
            farmId
        );

        _farmRepositoryMock
            .Setup(repository => repository.GetByIdAsync(farmId))
            .ReturnsAsync((Farm?)null);

        var action = async () => await _cropService.CreateAsync(request);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Farm not found.");

        _cropRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Crop>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCrops_WhenCropsExist()
    {
        var farmId = Guid.NewGuid();

        var crops = new List<Crop>
        {
            new("Soja", CropType.Soybean, 50, DateTime.UtcNow, farmId),
            new("Milho", CropType.Corn, 70, DateTime.UtcNow, farmId)
        };

        _cropRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(crops);

        var result = await _cropService.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Soja");
        result[1].Name.Should().Be("Milho");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCrop_WhenCropExists()
    {
        var cropId = Guid.NewGuid();
        var farmId = Guid.NewGuid();

        var crop = new Crop(
            "Soja",
            CropType.Soybean,
            50,
            DateTime.UtcNow,
            farmId
        );

        typeof(Crop)
            .BaseType!
            .GetProperty("Id")!
            .SetValue(crop, cropId);

        _cropRepositoryMock
            .Setup(repository => repository.GetByIdAsync(cropId))
            .ReturnsAsync(crop);

        var result = await _cropService.GetByIdAsync(cropId);

        result.Should().NotBeNull();
        result.Id.Should().Be(cropId);
        result.Name.Should().Be(crop.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenCropDoesNotExist()
    {
        var cropId = Guid.NewGuid();

        _cropRepositoryMock
            .Setup(repository => repository.GetByIdAsync(cropId))
            .ReturnsAsync((Crop?)null);

        var action = async () => await _cropService.GetByIdAsync(cropId);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Crop not found.");
    }

    [Fact]
    public async Task GetByFarmIdAsync_ShouldReturnCrops_WhenFarmExists()
    {
        var farmId = Guid.NewGuid();

        var farm = new Farm(
            "Fazenda Boa Esperança",
            "Gustavo Gomes",
            "São Paulo - SP",
            120.5
        );

        var crops = new List<Crop>
        {
            new("Soja", CropType.Soybean, 50, DateTime.UtcNow, farmId),
            new("Milho", CropType.Corn, 70, DateTime.UtcNow, farmId)
        };

        _farmRepositoryMock
            .Setup(repository => repository.GetByIdAsync(farmId))
            .ReturnsAsync(farm);

        _cropRepositoryMock
            .Setup(repository => repository.GetByFarmIdAsync(farmId))
            .ReturnsAsync(crops);

        var result = await _cropService.GetByFarmIdAsync(farmId);

        result.Should().HaveCount(2);
        result.All(crop => crop.FarmId == farmId).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCrop_WhenCropExists()
    {
        var cropId = Guid.NewGuid();
        var farmId = Guid.NewGuid();

        var crop = new Crop(
            "Soja",
            CropType.Soybean,
            50,
            DateTime.UtcNow,
            farmId
        );

        var request = new UpdateCropRequest(
            "Milho atualizado",
            CropType.Corn,
            80,
            DateTime.UtcNow
        );

        _cropRepositoryMock
            .Setup(repository => repository.GetByIdAsync(cropId))
            .ReturnsAsync(crop);

        _cropRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Crop>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _cropService.UpdateAsync(cropId, request);

        crop.Name.Should().Be(request.Name);
        crop.Type.Should().Be(request.Type);
        crop.AreaHectares.Should().Be(request.AreaHectares);
        crop.PlantingDate.Should().Be(request.PlantingDate);

        _cropRepositoryMock.Verify(
            repository => repository.UpdateAsync(crop),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCrop_WhenCropExists()
    {
        var cropId = Guid.NewGuid();
        var farmId = Guid.NewGuid();

        var crop = new Crop(
            "Soja",
            CropType.Soybean,
            50,
            DateTime.UtcNow,
            farmId
        );

        _cropRepositoryMock
            .Setup(repository => repository.GetByIdAsync(cropId))
            .ReturnsAsync(crop);

        _cropRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<Crop>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _cropService.DeleteAsync(cropId);

        _cropRepositoryMock.Verify(
            repository => repository.DeleteAsync(crop),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}