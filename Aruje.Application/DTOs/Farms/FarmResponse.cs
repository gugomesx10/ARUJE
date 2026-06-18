namespace Aruje.Application.DTOs.Farms;

public record FarmResponse(
    Guid Id,
    string Name,
    string OwnerName,
    string Location,
    double TotalAreaHectares,
    bool IsActive,
    DateTime CreatedAt
);