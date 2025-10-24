using backend.Application.Common.Interfaces;
using backend.Application.NotificationTemplates.Queries;
using backend.Application.NotificationTemplates.Queries.GetNotificationTemplateByEventType;
using backend.Domain.Entities;
using backend.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace backend.Application.Services;

/// <summary>
/// Triển khai dịch vụ gửi thông báo, sử dụng các mẫu thông báo và các kênh gửi khác nhau.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly IFirebaseNotificationService _firebaseNotificationService;
    private readonly IInAppNotificationService _inAppNotificationService;
    private readonly IApplicationDbContext _context;

    public NotificationService(
        ILogger<NotificationService> logger,
        IMediator mediator,
        IEmailService emailService,
        IFirebaseNotificationService firebaseNotificationService,
        IInAppNotificationService inAppNotificationService,
        IApplicationDbContext context)
    {
        _logger = logger;
        _mediator = mediator;
        _emailService = emailService;
        _firebaseNotificationService = firebaseNotificationService;
        _inAppNotificationService = inAppNotificationService;
        _context = context;
    }

    /// <summary>
    /// Gửi một thông báo dựa trên loại sự kiện, kênh và các placeholder.
    /// </summary>
    /// <param name="eventType">Loại sự kiện kích hoạt thông báo.</param>
    /// <param name="channel">Kênh thông báo sẽ được gửi.</param>
    /// <param name="placeholders">Từ điển chứa các giá trị thay thế cho placeholder trong mẫu.</param>
    /// <param name="recipientUserId">ID của người dùng nhận thông báo.</param>
    /// <param name="familyId">ID của gia đình liên quan (tùy chọn).</param>
    /// <param name="senderUserId">ID của người dùng gửi thông báo (tùy chọn).</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task SendNotificationAsync(
        NotificationType eventType,
        NotificationChannel channel,
        Dictionary<string, string> placeholders,
        string recipientUserId,
        Guid? familyId = null,
        string? senderUserId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to send notification for EventType: {EventType}, Channel: {Channel}, Recipient: {RecipientUserId}",
            eventType, channel, recipientUserId);

        // 1. Lấy mẫu thông báo
        var template = await _mediator.Send(new GetNotificationTemplateByEventTypeQuery
        {
            EventType = eventType,
            Channel = channel
        }, cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("No active notification template found for EventType: {EventType} and Channel: {Channel}. Notification not sent.",
                eventType, channel);
            return;
        }

        // 2. Render mẫu thông báo
        var renderedSubject = RenderTemplate(template.Subject, placeholders);
        var renderedBody = RenderTemplate(template.Body, placeholders);

        // 3. Tạo và lưu thông báo vào DB
        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            SenderUserId = senderUserId,
            Title = renderedSubject,
            Message = renderedBody,
            Type = eventType,
            FamilyId = familyId,
            Status = NotificationStatus.Pending // Ban đầu là Pending
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Gửi thông báo qua kênh phù hợp
        switch (channel)
        {
            case NotificationChannel.InApp:
                await _inAppNotificationService.SendInAppNotificationAsync(notification, cancellationToken);
                break;
            case NotificationChannel.Email:
                // TODO: Lấy email người nhận từ cơ sở dữ liệu hoặc dịch vụ khác
                // Tạm thời dùng một email giả
                await _emailService.SendEmailAsync(notification, "mock@example.com", cancellationToken);
                break;
            case NotificationChannel.Firebase:
                // TODO: Lấy device token từ cơ sở dữ liệu hoặc dịch vụ khác
                // Tạm thời dùng một device token giả
                await _firebaseNotificationService.SendPushNotificationAsync(notification, "mock_device_token", cancellationToken);
                break;
            case NotificationChannel.SMS:
                _logger.LogWarning("SMS channel not implemented. Skipping SMS notification for user {RecipientUserId}", recipientUserId);
                break;
            case NotificationChannel.Webhook:
                _logger.LogWarning("Webhook channel not implemented. Skipping Webhook notification for user {RecipientUserId}", recipientUserId);
                break;
            default:
                _logger.LogWarning("Unknown notification channel: {Channel}. Notification not sent.", channel);
                break;
        }

        // Cập nhật trạng thái thông báo sau khi gửi (có thể cần logic phức tạp hơn để xử lý lỗi từng kênh)
        notification.MarkAsSent();
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Thay thế các placeholder trong chuỗi bằng các giá trị cung cấp.
    /// Placeholder có định dạng {{Key}}.
    /// </summary>
    /// <param name="templateString">Chuỗi mẫu chứa các placeholder.</param>
    /// <param name="placeholders">Từ điển các cặp key-value để thay thế.</param>
    /// <returns>Chuỗi đã được render.</returns>
    private string RenderTemplate(string templateString, Dictionary<string, string> placeholders)
    {
        return Regex.Replace(templateString, @"\{\{(.*?)\}\}", match =>
        {
            var key = match.Groups[1].Value;
            return placeholders.TryGetValue(key, out var value) ? value : match.Value; // Giữ nguyên placeholder nếu không tìm thấy key
        });
    }
}