using backend.Domain.Entities;

namespace backend.Application.Services;

/// <summary>
/// Giao diện cho dịch vụ gửi email.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi một email thông báo.
    /// </summary>
    /// <param name="notification">Đối tượng thông báo cần gửi.</param>
    /// <param name="recipientEmail">Địa chỉ email của người nhận.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    Task SendEmailAsync(Notification notification, string recipientEmail, CancellationToken cancellationToken = default);
}
