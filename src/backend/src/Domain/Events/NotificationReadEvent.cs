using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events;

/// <summary>
/// Sự kiện miền được kích hoạt khi một thông báo được đánh dấu là đã đọc.
/// </summary>
public class NotificationReadEvent : BaseEvent
{
    /// <summary>
    /// Thông báo đã được đọc.
    /// </summary>
    public Notification Notification { get; }

    /// <summary>
    /// Khởi tạo một phiên bản mới của <see cref="NotificationReadEvent"/>.
    /// </summary>
    /// <param name="notification">Thông báo đã được đọc.</param>
    public NotificationReadEvent(Notification notification)
    {
        Notification = notification;
    }
}
