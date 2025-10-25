using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplates;

public record GetNotificationTemplatesQuery : IRequest<Result<PaginatedList<NotificationTemplateDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchQuery { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
    public NotificationType? EventType { get; init; }
    public NotificationChannel? Channel { get; init; }
    public TemplateFormat? Format { get; init; }
    public string? LanguageCode { get; init; }
    public bool? IsActive { get; init; }
}
