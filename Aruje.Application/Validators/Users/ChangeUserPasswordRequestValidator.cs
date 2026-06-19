using Aruje.Application.DTOs.Users;
using FluentValidation;

namespace Aruje.Application.Validators.Users;

public class ChangeUserPasswordRequestValidator : AbstractValidator<ChangeUserPasswordRequest>
{
    public ChangeUserPasswordRequestValidator()
    {
        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(80);
    }
}