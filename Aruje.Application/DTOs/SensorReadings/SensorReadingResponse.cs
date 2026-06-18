namespace Aruje.Application.DTOs.SensorReadings;

public record SensorReadingResponse(
    Guid Id,
    Guid SensorId,
    double? Temperature,
    double? AirHumidity,
    double? SoilMoisture,
    double? Luminosity,
    DateTime ReadingDate
);