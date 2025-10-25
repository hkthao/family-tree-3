using MediatR;
using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Queries;
using backend.Domain.Enums;

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

    /// <summary>
    /// Loại thông báo (ví dụ: General, FamilyCreated).
    /// </summary>
    public NotificationType NotificationType { get; init; }

    /// <summary>
    /// Kênh thông báo (ví dụ: InApp, Email).
    /// </summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// Mã ngôn ngữ cho nội dung được tạo (ví dụ: 'en', 'vi').
    /// </summary>
    public string LanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Định dạng của nội dung được tạo (ví dụ: PlainText, Html).
    /// </summary>
    public TemplateFormat Format { get; init; }
}
