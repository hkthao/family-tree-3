using backend.Domain.Common;
using backend.Domain.Enums;
using backend.Domain.Events;

namespace backend.Domain.Entities;

/// <summary>
/// Đại diện cho một thông báo trong hệ thống.
/// </summary>
public class Notification : BaseAuditableEntity
{
    /// <summary>
    /// Tiêu đề của thông báo.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung chi tiết của thông báo.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// ID của người dùng nhận thông báo.
    /// </summary>
    public string RecipientUserId { get; set; } = string.Empty;

    /// <summary>
    /// ID của người dùng gửi thông báo (nếu có).
    /// </summary>
    public string? SenderUserId { get; set; }

    /// <summary>
    /// Thời gian thông báo được đọc (nếu có).
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Trạng thái hiện tại của thông báo (Pending, Sent, Failed, Read).
    /// </summary>
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    /// <summary>
    /// Loại thông báo (e.g., NewFamilyMember, RelationshipConfirmed).
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.General;

    /// <summary>
    /// ID của gia đình liên quan đến thông báo (nếu có).
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Đánh dấu thông báo là đã đọc.
    /// </summary>
    public void MarkAsRead()
    {
        if (Status != NotificationStatus.Read)
        {
            Status = NotificationStatus.Read;
            ReadAt = DateTime.UtcNow;
            AddDomainEvent(new NotificationReadEvent(this));
        }
    }

    /// <summary>
    /// Đánh dấu thông báo là đã gửi.
    /// </summary>
    public void MarkAsSent()
    {
        if (Status == NotificationStatus.Pending)
        {
            Status = NotificationStatus.Sent;
            AddDomainEvent(new NotificationSentEvent(this));
        }
    }

    /// <summary>
    /// Đánh dấu thông báo là gửi thất bại.
    /// </summary>
    public void MarkAsFailed()
    {
        if (Status == NotificationStatus.Pending)
        {
            Status = NotificationStatus.Failed;
            AddDomainEvent(new NotificationFailedEvent(this));
        }
    }
}
