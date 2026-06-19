using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Crops;

public record UpdateCropRequest(
    string Name,
    CropType Type,
    double AreaHectares,
    DateTime PlantingDate
);