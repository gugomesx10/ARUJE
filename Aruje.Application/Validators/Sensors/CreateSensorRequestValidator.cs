using Aruje.Application.DTOs.Sensors;
using FluentValidation;

namespace Aruje.Application.Validators.Sensors;

public class CreateSensorRequestValidator : AbstractValidator<CreateSensorRequest>
{
    public CreateSensorRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.SerialNumber)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.CropId)
            .NotEmpty();
    }
}