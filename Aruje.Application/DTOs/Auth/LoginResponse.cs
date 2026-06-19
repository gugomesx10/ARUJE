using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Auth;

public record AuthResponse(
    Guid UserId,
    string FullName,
    string Email,
    UserRole Role,
    string Token
);