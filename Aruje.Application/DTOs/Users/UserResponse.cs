using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Users;

public record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive
);