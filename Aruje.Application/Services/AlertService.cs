using Aruje.Application.DTOs.Alerts;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;

namespace Aruje.Application.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;

    public AlertService(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

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

    public async Task<IReadOnlyList<AlertResponse>> GetAllAsync()
    {
        var alerts = await _alertRepository.GetAllAsync();

        return alerts.Select(alert => new AlertResponse(
            alert.Id,
            alert.Title,
            alert.Description,
            alert.Severity,
            alert.Status,
            alert.SensorReadingId,
            alert.CreatedAt
        )).ToList();
    }

    public async Task<IReadOnlyList<AlertResponse>> GetByStatusAsync(AlertStatus status)
    {
        var alerts = await _alertRepository.GetByStatusAsync(status);

        return alerts.Select(alert => new AlertResponse(
            alert.Id,
            alert.Title,
            alert.Description,
            alert.Severity,
            alert.Status,
            alert.SensorReadingId,
            alert.CreatedAt
        )).ToList();
    }

    public async Task<IReadOnlyList<AlertResponse>> GetBySeverityAsync(AlertSeverity severity)
    {
        var alerts = await _alertRepository.GetBySeverityAsync(severity);

        return alerts.Select(alert => new AlertResponse(
            alert.Id,
            alert.Title,
            alert.Description,
            alert.Severity,
            alert.Status,
            alert.SensorReadingId,
            alert.CreatedAt
        )).ToList();
    }
}