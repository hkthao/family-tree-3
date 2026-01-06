using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface INotificationService
{
    Task<Result> SyncSubscriberAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> SaveExpoPushTokenAsync(string userId, string expoPushToken, CancellationToken cancellationToken = default);
    Task<Result> DeleteExpoPushTokenAsync(string userId, string expoPushToken, CancellationToken cancellationToken = default);
    Task<Result> SendNotificationAsync(string workflowId, string userId, object payload, CancellationToken cancellationToken = default);
}
