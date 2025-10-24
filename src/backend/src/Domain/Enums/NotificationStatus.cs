namespace backend.Domain.Enums;

/// <summary>
/// Trạng thái của thông báo.
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Thông báo đang chờ xử lý hoặc gửi.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Thông báo đã được gửi thành công.
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Thông báo gửi thất bại.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Thông báo đã được người dùng đọc.
    /// </summary>
    Read = 3
}
