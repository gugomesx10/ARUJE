using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Services;

public interface IPushNotificationService
{
    Task SendAlertNotificationAsync(
        Alert alert,
        CancellationToken cancellationToken = default);
}