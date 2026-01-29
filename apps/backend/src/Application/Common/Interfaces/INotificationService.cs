using backend.Application.Common.Models;
using backend.Application.Notifications.DTOs;

namespace backend.Application.Common.Interfaces;

public interface INotificationService
{
    Task<Result> SyncSubscriberAsync(SyncSubscriberDto subscriberDto, CancellationToken cancellationToken = default);
    Task<Result> SaveExpoPushTokenAsync(string userId, List<string?> expoPushTokens, CancellationToken cancellationToken = default);
    Task<Result> DeleteExpoPushTokenAsync(string userId, string? expoPushToken, CancellationToken cancellationToken = default);
    Task<Result> SendNotificationAsync(string workflowId, string userId, object? payload, CancellationToken cancellationToken = default);
    Task<Result> SendNotificationAsync(string workflowId, List<string> recipientUserIds, object? payload, CancellationToken cancellationToken = default);
}
