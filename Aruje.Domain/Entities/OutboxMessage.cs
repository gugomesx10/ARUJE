using Aruje.Domain.Common;

namespace Aruje.Domain.Entities;

public class OutboxMessage : BaseEntity
{
    public string EventType { get; private set; }
    public string Payload { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    public bool IsProcessed => ProcessedAt.HasValue;

    private OutboxMessage()
    {
        EventType = string.Empty;
        Payload = string.Empty;
    }

    public OutboxMessage(string eventType, string payload)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.");

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.");

        EventType = eventType;
        Payload = payload;
        OccurredAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
        MarkAsUpdated();
    }

    public void MarkAsFailed(string error)
    {
        RetryCount++;
        Error = error;
        MarkAsUpdated();
    }
}