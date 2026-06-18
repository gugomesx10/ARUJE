using Aruje.Domain.Common;
using Aruje.Domain.Enums;

namespace Aruje.Domain.Entities;

public class Alert : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }

    public AlertSeverity Severity { get; private set; }
    public AlertStatus Status { get; private set; }

    public Guid SensorReadingId { get; private set; }

    private Alert()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Alert(
        string title,
        string description,
        AlertSeverity severity,
        Guid sensorReadingId)
    {
        Validate(title, description, sensorReadingId);

        Title = title;
        Description = description;
        Severity = severity;
        Status = AlertStatus.Open;
        SensorReadingId = sensorReadingId;
    }

    public void Resolve()
    {
        Status = AlertStatus.Resolved;
        MarkAsUpdated();
    }

    public void StartProcessing()
    {
        Status = AlertStatus.InProgress;
        MarkAsUpdated();
    }

    public void Close()
    {
        Status = AlertStatus.Closed;
        MarkAsUpdated();
    }

    private static void Validate(
        string title,
        string description,
        Guid sensorReadingId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Alert title is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Alert description is required.");

        if (sensorReadingId == Guid.Empty)
            throw new ArgumentException("SensorReadingId is required.");
    }
}