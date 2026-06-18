using Aruje.Domain.Enums;

namespace Aruje.Application.DTOs.Alerts;

public record AlertResponse(
    Guid Id,
    string Title,
    string Description,
    AlertSeverity Severity,
    AlertStatus Status,
    Guid SensorReadingId,
    DateTime CreatedAt
);