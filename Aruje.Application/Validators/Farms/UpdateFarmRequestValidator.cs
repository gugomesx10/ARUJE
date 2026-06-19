using Aruje.Application.DTOs.Farms;
using FluentValidation;

namespace Aruje.Application.Validators.Farms;

public class UpdateFarmRequestValidator : AbstractValidator<UpdateFarmRequest>
{
    public UpdateFarmRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(request => request.OwnerName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(request => request.Location)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.TotalAreaHectares)
            .GreaterThan(0);
    }
}