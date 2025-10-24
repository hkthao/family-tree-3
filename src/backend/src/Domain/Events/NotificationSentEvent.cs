using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events;

/// <summary>
/// Sự kiện miền được kích hoạt khi một thông báo được đánh dấu là đã gửi.
/// </summary>
public class NotificationSentEvent : BaseEvent
{
    /// <summary>
    /// Thông báo đã được gửi.
    /// </summary>
    public Notification Notification { get; }

    /// <summary>
    /// Khởi tạo một phiên bản mới của <see cref="NotificationSentEvent"/>.
    /// </summary>
    /// <param name="notification">Thông báo đã được gửi.</param>
    public NotificationSentEvent(Notification notification)
    {
        Notification = notification;
    }
}
