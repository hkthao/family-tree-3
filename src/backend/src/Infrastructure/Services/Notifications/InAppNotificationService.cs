using backend.Application.Services;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services.Notifications;

/// <summary>
/// Triển khai dịch vụ gửi thông báo trong ứng dụng, sử dụng INotificationHubService để gửi thông báo theo thời gian thực.
/// </summary>
public class InAppNotificationService : IInAppNotificationService
{
    private readonly ILogger<InAppNotificationService> _logger;
    private readonly INotificationHubService _notificationHubService;

    public InAppNotificationService(ILogger<InAppNotificationService> logger, INotificationHubService notificationHubService)
    {
        _logger = logger;
        _notificationHubService = notificationHubService;
    }

    /// <summary>
    /// Gửi một thông báo trong ứng dụng đến người dùng cụ thể thông qua SignalR.
    /// </summary>
    /// <param name="notification">Đối tượng thông báo cần gửi.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task SendInAppNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending in-app notification to user {RecipientUserId}. Title: {Title}, Message: {Message}",
            notification.RecipientUserId, notification.Title, notification.Message);

        await _notificationHubService.SendNotificationToUserAsync(
            notification.RecipientUserId,
            notification.Title,
            notification.Message,
            notification.Type.ToString(),
            cancellationToken);
    }
}
