using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Sensors;

public record CreateSensorRequest(
    string Name,
    SensorType Type,
    string SerialNumber,
    Guid CropId
);