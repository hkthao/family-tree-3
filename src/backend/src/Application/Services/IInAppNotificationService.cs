using backend.Domain.Entities;

namespace backend.Application.Services;

/// <summary>
/// Giao diện cho dịch vụ gửi thông báo trong ứng dụng.
/// </summary>
public interface IInAppNotificationService
{
    /// <summary>
    /// Gửi một thông báo trong ứng dụng đến người dùng cụ thể.
    /// </summary>
    /// <param name="notification">Đối tượng thông báo cần gửi.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    Task SendInAppNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
}
