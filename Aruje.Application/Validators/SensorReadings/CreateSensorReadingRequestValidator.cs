using Aruje.Application.DTOs.SensorReadings;
using FluentValidation;

namespace Aruje.Application.Validators.SensorReadings;

public class CreateSensorReadingRequestValidator : AbstractValidator<CreateSensorReadingRequest>
{
    public CreateSensorReadingRequestValidator()
    {
        RuleFor(x => x.SensorId)
            .NotEmpty();

        RuleFor(x => x.ReadingDate)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x =>
                x.Temperature.HasValue ||
                x.AirHumidity.HasValue ||
                x.SoilMoisture.HasValue ||
                x.Luminosity.HasValue)
            .WithMessage("At least one sensor value is required.");
    }
}