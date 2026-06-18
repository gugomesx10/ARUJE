using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Crops;

public record CropResponse(
    Guid Id,
    string Name,
    CropType Type,
    double AreaHectares,
    DateTime PlantingDate,
    Guid FarmId,
    bool IsActive
);