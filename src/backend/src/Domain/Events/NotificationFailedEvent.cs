using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events;

/// <summary>
/// Sự kiện miền được kích hoạt khi một thông báo được đánh dấu là gửi thất bại.
/// </summary>
public class NotificationFailedEvent : BaseEvent
{
    /// <summary>
    /// Thông báo gửi thất bại.
    /// </summary>
    public Notification Notification { get; }

    /// <summary>
    /// Khởi tạo một phiên bản mới của <see cref="NotificationFailedEvent"/>.
    /// </summary>
    /// <param name="notification">Thông báo gửi thất bại.</param>
    public NotificationFailedEvent(Notification notification)
    {
        Notification = notification;
    }
}
