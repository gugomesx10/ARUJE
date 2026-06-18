using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Sensors;

public record SensorResponse(
    Guid Id,
    string Name,
    SensorType Type,
    string SerialNumber,
    Guid CropId,
    bool IsActive
);