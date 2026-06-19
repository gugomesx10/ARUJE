using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Users;

public record ChangeUserRoleRequest(
    UserRole Role
);