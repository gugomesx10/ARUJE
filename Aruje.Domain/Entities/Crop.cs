using Aruje.Domain.Common;
using Aruje.Domain.Enums;

namespace Aruje.Domain.Entities;

public class Crop : BaseEntity
{
    public string Name { get; private set; }
    public CropType Type { get; private set; }
    public double AreaHectares { get; private set; }
    public DateTime PlantingDate { get; private set; }
    public Guid FarmId { get; private set; }

    private Crop()
    {
        Name = string.Empty;
    }

    public Crop(string name, CropType type, double areaHectares, DateTime plantingDate, Guid farmId)
    {
        Validate(name, areaHectares, farmId);

        Name = name;
        Type = type;
        AreaHectares = areaHectares;
        PlantingDate = plantingDate;
        FarmId = farmId;
    }

    public void Update(string name, CropType type, double areaHectares, DateTime plantingDate)
    {
        Validate(name, areaHectares, FarmId);

        Name = name;
        Type = type;
        AreaHectares = areaHectares;
        PlantingDate = plantingDate;

        MarkAsUpdated();
    }

    private static void Validate(string name, double areaHectares, Guid farmId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Crop name is required.");

        if (areaHectares <= 0)
            throw new ArgumentException("Crop area must be greater than zero.");

        if (farmId == Guid.Empty)
            throw new ArgumentException("FarmId is required.");
    }
}