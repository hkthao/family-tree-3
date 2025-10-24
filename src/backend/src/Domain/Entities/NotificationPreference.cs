using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Đại diện cho tùy chọn thông báo của người dùng cho một kênh cụ thể.
/// </summary>
public class NotificationPreference : BaseAuditableEntity
{
    /// <summary>
    /// ID của người dùng sở hữu tùy chọn này.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Kênh thông báo mà tùy chọn này áp dụng.
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Cho biết liệu kênh thông báo này có được bật cho người dùng hay không.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Loại thông báo mà tùy chọn này áp dụng (ví dụ: NewFamilyMember, SystemAlert).
    /// Nếu null, áp dụng cho tất cả các loại thông báo trên kênh này.
    /// </summary>
    public NotificationType? NotificationType { get; set; }
}
