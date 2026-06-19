using Aruje.Application.DTOs.Sensors;
using FluentValidation;

namespace Aruje.Application.Validators.Sensors;

public class UpdateSensorRequestValidator : AbstractValidator<UpdateSensorRequest>
{
    public UpdateSensorRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(request => request.Type)
            .IsInEnum();

        RuleFor(request => request.SerialNumber)
            .NotEmpty()
            .MaximumLength(80);
    }
}