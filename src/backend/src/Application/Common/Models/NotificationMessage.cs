using backend.Domain.Enums;

namespace backend.Application.Common.Models;

/// <summary>
/// Đại diện cho một tin nhắn thông báo sẽ được gửi.
/// </summary>
public class NotificationMessage
{
    /// <summary>
    /// ID của người dùng nhận thông báo.
    /// </summary>
    public string RecipientUserId { get; set; } = string.Empty;

    /// <summary>
    /// ID của người dùng gửi thông báo (nếu có).
    /// </summary>
    public string? SenderUserId { get; set; }

    /// <summary>
    /// Tiêu đề của thông báo.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung chi tiết của thông báo.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Loại thông báo.
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.General;

    /// <summary>
    /// ID của gia đình liên quan đến thông báo (nếu có).
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Các kênh mà thông báo này nên được gửi đến.
    /// Nếu null hoặc rỗng, dịch vụ sẽ sử dụng tùy chọn mặc định hoặc tùy chọn của người dùng.
    /// </summary>
    public List<NotificationChannel>? PreferredChannels { get; set; }

    /// <summary>
    /// Dữ liệu bổ sung dưới dạng JSON để thông báo có thể sử dụng.
    /// </summary>
    public string? Data { get; set; }
}