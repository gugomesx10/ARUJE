using Aruje.Application.Messaging;

namespace Aruje.Application.Interfaces.Messaging;

public interface IMessagePublisher
{
    Task PublishSensorReadingCreatedAsync(
        SensorReadingCreatedMessage message,
        CancellationToken cancellationToken = default);
}