namespace Aruje.Application.DTOs.SensorReadings;

public record CreateSensorReadingRequest(
    Guid SensorId,
    double? Temperature,
    double? AirHumidity,
    double? SoilMoisture,
    double? Luminosity,
    DateTime ReadingDate
);