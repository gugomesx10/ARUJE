namespace Aruje.Application.DTOs.Farms;

public record UpdateFarmRequest(
    string Name,
    string OwnerName,
    string Location,
    double TotalAreaHectares
);