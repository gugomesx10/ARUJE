using Aruje.Application.DTOs.Farms;
using FluentValidation;

namespace Aruje.Application.Validators.Farms;

public class CreateFarmRequestValidator : AbstractValidator<CreateFarmRequest>
{
    public CreateFarmRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.OwnerName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TotalAreaHectares)
            .GreaterThan(0);
    }
}