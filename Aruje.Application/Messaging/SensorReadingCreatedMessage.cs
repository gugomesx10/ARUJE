namespace Aruje.Application.Messaging;

public record SensorReadingCreatedMessage(
    Guid SensorReadingId,
    Guid SensorId,
    double? Temperature,
    double? AirHumidity,
    double? SoilMoisture,
    double? Luminosity,
    DateTime ReadingDate,
    DateTime CreatedAt
);