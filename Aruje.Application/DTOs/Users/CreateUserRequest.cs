using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Users;

public record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role
);