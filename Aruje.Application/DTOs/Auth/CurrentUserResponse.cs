namespace Aruje.Application.DTOs.Auth;

public record CurrentUserResponse(
    Guid UserId,
    string FullName,
    string Email,
    string Role
);