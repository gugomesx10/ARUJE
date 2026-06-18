using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Crops;

public record CreateCropRequest(
    string Name,
    CropType Type,
    double AreaHectares,
    DateTime PlantingDate,
    Guid FarmId
);