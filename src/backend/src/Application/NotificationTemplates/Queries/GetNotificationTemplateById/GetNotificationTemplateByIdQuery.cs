using backend.Application.Common.Models;

namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplateById;

public record GetNotificationTemplateByIdQuery(Guid Id) : IRequest<Result<NotificationTemplateDto>>;
