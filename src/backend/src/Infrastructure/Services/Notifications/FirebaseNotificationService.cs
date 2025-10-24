using backend.Application.Services;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services.Notifications;

/// <summary>
/// Triển khai dịch vụ gửi thông báo qua Firebase (hiện tại là mock).
/// </summary>
public class FirebaseNotificationService : IFirebaseNotificationService
{
    private readonly ILogger<FirebaseNotificationService> _logger;

    public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gửi một thông báo đẩy đến thiết bị di động qua Firebase.
    /// </summary>
    /// <param name="notification">Đối tượng thông báo cần gửi.</param>
    /// <param name="deviceToken">Token thiết bị Firebase của người nhận.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public Task SendPushNotificationAsync(Notification notification, string deviceToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock Firebase Notification: Sending push notification to {DeviceToken} for user {RecipientUserId}. Title: {Title}, Message: {Message}",
            deviceToken, notification.RecipientUserId, notification.Title, notification.Message);
        // TODO: Implement actual Firebase Admin SDK logic here
        return Task.CompletedTask;
    }
}
