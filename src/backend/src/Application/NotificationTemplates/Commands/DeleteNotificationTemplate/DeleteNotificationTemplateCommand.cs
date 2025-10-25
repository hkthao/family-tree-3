using backend.Application.Common.Models;

namespace backend.Application.NotificationTemplates.Commands.DeleteNotificationTemplate;

public record DeleteNotificationTemplateCommand(Guid Id) : IRequest<Result<Unit>>;
