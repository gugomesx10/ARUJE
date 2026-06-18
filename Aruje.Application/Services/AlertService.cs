using Aruje.Application.Interfaces.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;

namespace Aruje.Application.Services;

public class AlertService : IAlertService
{
    public Task<Alert?> GenerateAlertFromReadingAsync(
        SensorReading reading,
        CancellationToken cancellationToken = default)
    {
        if (reading.Temperature >= 38 && reading.SoilMoisture <= 25)
        {
            var alert = new Alert(
                "Risco de estresse hídrico",
                "Temperatura elevada combinada com baixa umidade do solo.",
                AlertSeverity.High,
                reading.Id
            );

            return Task.FromResult<Alert?>(alert);
        }

        if (reading.Temperature >= 35)
        {
            var alert = new Alert(
                "Temperatura elevada",
                "A leitura indica temperatura acima do recomendado.",
                AlertSeverity.Medium,
                reading.Id
            );

            return Task.FromResult<Alert?>(alert);
        }

        if (reading.SoilMoisture <= 20)
        {
            var alert = new Alert(
                "Baixa umidade do solo",
                "A leitura indica baixa umidade do solo.",
                AlertSeverity.High,
                reading.Id
            );

            return Task.FromResult<Alert?>(alert);
        }

        return Task.FromResult<Alert?>(null);
    }
}