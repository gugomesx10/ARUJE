using Aruje.Application.DTOs.Users;
using FluentValidation;

namespace Aruje.Application.Validators.Users;

public class ChangeUserRoleRequestValidator : AbstractValidator<ChangeUserRoleRequest>
{
    public ChangeUserRoleRequestValidator()
    {
        RuleFor(request => request.Role)
            .IsInEnum();
    }
}