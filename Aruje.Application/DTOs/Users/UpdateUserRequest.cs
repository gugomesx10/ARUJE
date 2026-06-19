namespace Aruje.Application.DTOs.Users;

public record UpdateUserRequest(
    string FullName,
    string Email
);