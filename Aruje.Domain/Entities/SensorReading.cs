using Aruje.Domain.Common;

namespace Aruje.Domain.Entities;

public class SensorReading : BaseEntity
{
    public Guid SensorId { get; private set; }

    public double? Temperature { get; private set; }
    public double? AirHumidity { get; private set; }
    public double? SoilMoisture { get; private set; }
    public double? Luminosity { get; private set; }

    public DateTime ReadingDate { get; private set; }

    private SensorReading()
    {
    }

    public SensorReading(
        Guid sensorId,
        double? temperature,
        double? airHumidity,
        double? soilMoisture,
        double? luminosity,
        DateTime readingDate)
    {
        Validate(sensorId, temperature, airHumidity, soilMoisture, luminosity, readingDate);

        SensorId = sensorId;
        Temperature = temperature;
        AirHumidity = airHumidity;
        SoilMoisture = soilMoisture;
        Luminosity = luminosity;
        ReadingDate = readingDate;
    }

    private static void Validate(
        Guid sensorId,
        double? temperature,
        double? airHumidity,
        double? soilMoisture,
        double? luminosity,
        DateTime readingDate)
    {
        if (sensorId == Guid.Empty)
            throw new ArgumentException("SensorId is required.");

        if (readingDate == default)
            throw new ArgumentException("Reading date is required.");

        if (temperature is null &&
            airHumidity is null &&
            soilMoisture is null &&
            luminosity is null)
            throw new ArgumentException("At least one sensor value is required.");
    }
}