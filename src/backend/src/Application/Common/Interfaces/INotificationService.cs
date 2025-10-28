using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Giao diện cho dịch vụ gửi thông báo cấp ứng dụng, sử dụng các nhà cung cấp thông báo bên ngoài.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Gửi một tin nhắn thông báo thông qua các nhà cung cấp thông báo đã đăng ký.
    /// </summary>
    /// <param name="message">Tin nhắn thông báo chứa thông tin người nhận, nội dung và metadata.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    Task SendNotificationAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
