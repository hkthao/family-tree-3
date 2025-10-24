using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendNotification(NotificationMessage message, CancellationToken cancellationToken = default);
}
