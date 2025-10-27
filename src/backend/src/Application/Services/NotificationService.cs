using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Services;

/// <summary>
/// Triển khai dịch vụ gửi thông báo, sử dụng các mẫu thông báo và các kênh gửi khác nhau.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationProvider _notificationProvider;

    /// <summary>
    /// Khởi tạo một phiên bản mới của NotificationService.
    /// </summary>
    /// <param name="notificationProvider">Nhà cung cấp thông báo để gửi tin nhắn.</param>
    public NotificationService(INotificationProvider notificationProvider)
    {
        _notificationProvider = notificationProvider;
    }

    /// <summary>
    /// Gửi một tin nhắn thông báo thông qua nhà cung cấp thông báo đã đăng ký.
    /// </summary>
    /// <param name="message">Tin nhắn thông báo chứa thông tin người nhận, nội dung và metadata.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public Task SendNotificationAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        return _notificationProvider.SendAsync(message, cancellationToken);
    }
}