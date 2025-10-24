using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Application.Services;

/// <summary>
/// Triển khai dịch vụ gửi thông báo, quản lý việc lưu trữ và phân phối thông báo đến các kênh khác nhau.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IFirebaseNotificationService _firebaseNotificationService;
    private readonly IEmailService _emailService;
    private readonly IInAppNotificationService _inAppNotificationService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        IUser user,
        IFirebaseNotificationService firebaseNotificationService,
        IEmailService emailService,
        IInAppNotificationService inAppNotificationService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _user = user;
        _firebaseNotificationService = firebaseNotificationService;
        _emailService = emailService;
        _inAppNotificationService = inAppNotificationService;
        _logger = logger;
    }

    /// <summary>
    /// Gửi một thông báo đến người dùng, lưu trữ vào cơ sở dữ liệu và phân phối qua các kênh đã cấu hình.
    /// </summary>
    /// <param name="message">Thông điệp thông báo.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task SendNotification(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        // 1. Lưu thông báo vào cơ sở dữ liệu
        var notification = new Notification
        {
            RecipientUserId = message.RecipientUserId,
            SenderUserId = message.SenderUserId,
            Title = message.Title,
            Message = message.Message,
            Type = message.Type,
            FamilyId = message.FamilyId,
            Status = NotificationStatus.Pending // Ban đầu là Pending
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Lấy tùy chọn thông báo của người dùng
        var userPreferences = await _context.NotificationPreferences
            .Where(p => p.UserId == message.RecipientUserId)
            .ToListAsync(cancellationToken);

        // 3. Quyết định kênh nào sẽ sử dụng
        var channelsToSend = new List<NotificationChannel>();

        if (message.PreferredChannels != null && message.PreferredChannels.Any())
        {
            // Nếu tin nhắn chỉ định kênh ưu tiên, sử dụng chúng
            channelsToSend.AddRange(message.PreferredChannels);
        }
        else
        {
            // Nếu không, sử dụng tùy chọn của người dùng hoặc mặc định
            foreach (NotificationChannel channel in Enum.GetValues(typeof(NotificationChannel)))
            {
                var preference = userPreferences.FirstOrDefault(p => p.Channel == channel && (p.NotificationType == null || p.NotificationType == message.Type));
                if (preference?.Enabled ?? true) // Mặc định là bật nếu không có tùy chọn cụ thể
                {
                    channelsToSend.Add(channel);
                }
            }
        }

        // 4. Gửi thông báo qua các kênh đã chọn
        var sendTasks = new List<Task>();
        foreach (var channel in channelsToSend.Distinct())
        {
            switch (channel)
            {
                case NotificationChannel.InApp:
                    sendTasks.Add(_inAppNotificationService.SendInAppNotificationAsync(notification, cancellationToken));
                    break;
                case NotificationChannel.Firebase:
                    // TODO: Lấy device token từ cơ sở dữ liệu hoặc dịch vụ khác
                    // Tạm thời bỏ qua hoặc dùng một device token giả
                    _logger.LogWarning("Firebase device token not implemented. Skipping Firebase notification for user {RecipientUserId}", message.RecipientUserId);
                    // sendTasks.Add(_firebaseNotificationService.SendPushNotificationAsync(notification, "mock_device_token", cancellationToken));
                    break;
                case NotificationChannel.Email:
                    // TODO: Lấy email người nhận từ cơ sở dữ liệu hoặc dịch vụ khác
                    // Tạm thời bỏ qua hoặc dùng một email giả
                    _logger.LogWarning("Recipient email not implemented. Skipping Email notification for user {RecipientUserId}", message.RecipientUserId);
                    // sendTasks.Add(_emailService.SendEmailAsync(notification, "mock@example.com", cancellationToken));
                    break;
                case NotificationChannel.SMS:
                    _logger.LogWarning("SMS channel not implemented. Skipping SMS notification for user {RecipientUserId}", message.RecipientUserId);
                    break;
                case NotificationChannel.Webhook:
                    _logger.LogWarning("Webhook channel not implemented. Skipping Webhook notification for user {RecipientUserId}", message.RecipientUserId);
                    break;
            }
        }

        await Task.WhenAll(sendTasks);

        // Cập nhật trạng thái thông báo sau khi gửi (có thể cần logic phức tạp hơn để xử lý lỗi từng kênh)
        notification.MarkAsSent();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
