using backend.Application.Services;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services.Notifications;

/// <summary>
/// Triển khai dịch vụ gửi email (hiện tại là mock).
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gửi một email thông báo.
    /// </summary>
    /// <param name="notification">Đối tượng thông báo cần gửi.</param>
    /// <param name="recipientEmail">Địa chỉ email của người nhận.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public Task SendEmailAsync(Notification notification, string recipientEmail, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock Email Service: Sending email to {RecipientEmail} for user {RecipientUserId}. Subject: {Title}, Body: {Message}",
            recipientEmail, notification.RecipientUserId, notification.Title, notification.Message);
        // TODO: Implement actual SMTP client logic here
        return Task.CompletedTask;
    }
}
