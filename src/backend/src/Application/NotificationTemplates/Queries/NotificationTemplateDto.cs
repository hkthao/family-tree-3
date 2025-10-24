using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.NotificationTemplates.Queries;

/// <summary>
/// DTO cho mẫu thông báo.
/// </summary>
public class NotificationTemplateDto : IMapFrom<NotificationTemplate>
{
    /// <summary>
    /// ID của mẫu thông báo.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Loại sự kiện mà mẫu thông báo này áp dụng.
    /// </summary>
    public NotificationType EventType { get; init; }

    /// <summary>
    /// Kênh thông báo mà mẫu này được sử dụng.
    /// </summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// Chủ đề của thông báo.
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Nội dung chính của thông báo.
    /// </summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>
    /// Cho biết mẫu thông báo này có đang hoạt động hay không.
    /// </summary>
    public bool IsActive { get; init; }
}
