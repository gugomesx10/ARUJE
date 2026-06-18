namespace Aruje.Application.DTOs.Farms;

public record CreateFarmRequest(
    string Name,
    string OwnerName,
    string Location,
    double TotalAreaHectares
);