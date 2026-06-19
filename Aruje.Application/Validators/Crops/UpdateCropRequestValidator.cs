using Aruje.Application.DTOs.Crops;
using FluentValidation;

namespace Aruje.Application.Validators.Crops;

public class UpdateCropRequestValidator : AbstractValidator<UpdateCropRequest>
{
    public UpdateCropRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(request => request.Type)
            .IsInEnum();

        RuleFor(request => request.AreaHectares)
            .GreaterThan(0);

        RuleFor(request => request.PlantingDate)
            .NotEmpty();
    }
}