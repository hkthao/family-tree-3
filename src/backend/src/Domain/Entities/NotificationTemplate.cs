using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Đại diện cho một mẫu thông báo có thể tái sử dụng.
/// </summary>
public class NotificationTemplate : BaseAuditableEntity
{
    /// <summary>
    /// Loại sự kiện mà mẫu thông báo này áp dụng (ví dụ: NewFamilyMember, RelationshipConfirmed).
    /// </summary>
    public NotificationType EventType { get; set; }

    /// <summary>
    /// Kênh thông báo mà mẫu này được sử dụng (ví dụ: InApp, Email, Firebase).
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Chủ đề của thông báo (đối với Email) hoặc tiêu đề (đối với InApp/Firebase).
    /// Có thể chứa các placeholder.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung chính của thông báo. Có thể chứa các placeholder.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Cho biết mẫu thông báo này có đang hoạt động hay không.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
