using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Giao diện cho dịch vụ gửi thông báo, sử dụng các mẫu thông báo.
/// </summary>
public interface INotificationService
{
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
    Task SendNotificationAsync(
        NotificationType eventType,
        NotificationChannel channel,
        Dictionary<string, string> placeholders,
        string recipientUserId,
        Guid? familyId = null,
        string? senderUserId = null,
        CancellationToken cancellationToken = default);
}
