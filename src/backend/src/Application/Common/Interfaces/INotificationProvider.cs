
namespace backend.Application.Common.Interfaces;

using backend.Application.Common.Models;

/// <summary>
/// Định nghĩa giao diện cho một nhà cung cấp thông báo bên ngoài.
/// </summary>
public interface INotificationProvider
{
    /// <summary>
    /// Gửi một tin nhắn thông báo thông qua nhà cung cấp này.
    /// </summary>
    /// <param name="message">Tin nhắn thông báo chứa thông tin người nhận, nội dung và metadata.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken);
}
