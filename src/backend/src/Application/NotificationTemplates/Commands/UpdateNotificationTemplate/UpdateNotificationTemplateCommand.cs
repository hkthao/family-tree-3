using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.NotificationTemplates.Commands.UpdateNotificationTemplate;

public record UpdateNotificationTemplateCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public NotificationType EventType { get; init; }
    public NotificationChannel Channel { get; init; }
    public string Subject { get; init; } = null!;
    public string Body { get; init; } = null!;
    public TemplateFormat Format { get; init; }
    public string LanguageCode { get; init; } = null!;
    public bool IsActive { get; init; }
}
