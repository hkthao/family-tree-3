using backend.Domain.Entities;

namespace backend.Application.Services;

/// <summary>
/// Giao diện cho dịch vụ gửi thông báo qua Firebase.
/// </summary>
public interface IFirebaseNotificationService
{
    /// <summary>
    /// Gửi một thông báo đẩy đến thiết bị di động qua Firebase.
    /// </summary>
    /// <param name="notification">Đối tượng thông báo cần gửi.</param>
    /// <param name="deviceToken">Token thiết bị Firebase của người nhận.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    Task SendPushNotificationAsync(Notification notification, string deviceToken, CancellationToken cancellationToken = default);
}
