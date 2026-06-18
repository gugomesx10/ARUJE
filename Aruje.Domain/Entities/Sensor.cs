using Aruje.Domain.Common;
using Aruje.Domain.Enums;

namespace Aruje.Domain.Entities;

public class Sensor : BaseEntity
{
    public string Name { get; private set; }
    public SensorType Type { get; private set; }
    public string SerialNumber { get; private set; }
    public Guid CropId { get; private set; }

    private Sensor()
    {
        Name = string.Empty;
        SerialNumber = string.Empty;
    }

    public Sensor(
        string name,
        SensorType type,
        string serialNumber,
        Guid cropId)
    {
        Validate(name, serialNumber, cropId);

        Name = name;
        Type = type;
        SerialNumber = serialNumber;
        CropId = cropId;
    }

    public void Update(
        string name,
        SensorType type,
        string serialNumber)
    {
        Validate(name, serialNumber, CropId);

        Name = name;
        Type = type;
        SerialNumber = serialNumber;

        MarkAsUpdated();
    }

    private static void Validate(
        string name,
        string serialNumber,
        Guid cropId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sensor name is required.");

        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number is required.");

        if (cropId == Guid.Empty)
            throw new ArgumentException("CropId is required.");
    }
}