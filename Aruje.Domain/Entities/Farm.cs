using Aruje.Domain.Common;

namespace Aruje.Domain.Entities;

public class Farm : BaseEntity
{
    public string Name { get; private set; }
    public string OwnerName { get; private set; }
    public string Location { get; private set; }
    public double TotalAreaHectares { get; private set; }

    private Farm()
    {
        Name = string.Empty;
        OwnerName = string.Empty;
        Location = string.Empty;
    }

    public Farm(string name, string ownerName, string location, double totalAreaHectares)
    {
        Validate(name, ownerName, location, totalAreaHectares);

        Name = name;
        OwnerName = ownerName;
        Location = location;
        TotalAreaHectares = totalAreaHectares;
    }

    public void Update(string name, string ownerName, string location, double totalAreaHectares)
    {
        Validate(name, ownerName, location, totalAreaHectares);

        Name = name;
        OwnerName = ownerName;
        Location = location;
        TotalAreaHectares = totalAreaHectares;

        MarkAsUpdated();
    }

    private static void Validate(string name, string ownerName, string location, double totalAreaHectares)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Farm name is required.");

        if (string.IsNullOrWhiteSpace(ownerName))
            throw new ArgumentException("Owner name is required.");

        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Farm location is required.");

        if (totalAreaHectares <= 0)
            throw new ArgumentException("Total area must be greater than zero.");
    }
}