using Aruje.Application.DTOs.Crops;
using FluentValidation;

namespace Aruje.Application.Validators.Crops;

public class CreateCropRequestValidator : AbstractValidator<CreateCropRequest>
{
    public CreateCropRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.AreaHectares)
            .GreaterThan(0);

        RuleFor(x => x.PlantingDate)
            .NotEmpty();

        RuleFor(x => x.FarmId)
            .NotEmpty();
    }
}