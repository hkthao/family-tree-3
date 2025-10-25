using backend.Application.Common.Models;
using backend.Domain.Enums;
using MediatR;

namespace backend.Application.NotificationTemplates.Commands.CreateNotificationTemplate;

public record CreateNotificationTemplateCommand : IRequest<Result<Guid>>
{
    public NotificationType EventType { get; init; }
    public NotificationChannel Channel { get; init; }
    public string Subject { get; init; } = null!;
    public string Body { get; init; } = null!;
    public TemplateFormat Format { get; init; }
    public string LanguageCode { get; init; } = "en";
    public bool IsActive { get; init; } = true;
}
