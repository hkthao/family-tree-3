namespace backend.Application.NotificationTemplates.Queries;

/// <summary>
/// DTO chứa nội dung và chủ đề được tạo bởi AI cho mẫu thông báo.
/// </summary>
public class GeneratedNotificationTemplateContentDto
{
    /// <summary>
    /// Chủ đề được tạo bởi AI.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung (body) được tạo bởi AI.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
