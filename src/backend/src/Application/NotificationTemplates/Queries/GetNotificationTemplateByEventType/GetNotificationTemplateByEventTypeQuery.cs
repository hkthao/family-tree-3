using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplateByEventType;

/// <summary>
/// Truy vấn để lấy mẫu thông báo dựa trên loại sự kiện và kênh.
/// </summary>
public record GetNotificationTemplateByEventTypeQuery : IRequest<Result<NotificationTemplateDto>>
{
    /// <summary>
    /// Loại sự kiện của mẫu thông báo.
    /// </summary>
    public NotificationType EventType { get; init; }

    /// <summary>
    /// Kênh thông báo của mẫu thông báo.
    /// </summary>
    public NotificationChannel Channel { get; init; }
}


