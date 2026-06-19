using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Sensors;

public record UpdateSensorRequest(
    string Name,
    SensorType Type,
    string SerialNumber
);