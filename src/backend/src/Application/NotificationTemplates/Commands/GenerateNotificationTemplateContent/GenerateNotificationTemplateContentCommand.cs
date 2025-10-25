using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Queries;

namespace backend.Application.NotificationTemplates.Commands.GenerateNotificationTemplateContent;

/// <summary>
/// Lệnh để tạo nội dung và chủ đề cho mẫu thông báo bằng AI.
/// </summary>
public record GenerateNotificationTemplateContentCommand : IRequest<Result<GeneratedNotificationTemplateContentDto>>
{
    /// <summary>
    /// Lời nhắc (prompt) cho AI để tạo nội dung.
    /// </summary>
    public string Prompt { get; init; } = string.Empty;
}
