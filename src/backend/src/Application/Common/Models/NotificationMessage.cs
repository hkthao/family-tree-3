
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
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu bổ sung dưới dạng Dictionary để thông báo có thể sử dụng.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}