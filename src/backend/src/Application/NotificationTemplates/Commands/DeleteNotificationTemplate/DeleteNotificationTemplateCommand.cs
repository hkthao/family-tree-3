using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.NotificationTemplates.Commands.DeleteNotificationTemplate;

public record DeleteNotificationTemplateCommand(Guid Id) : IRequest<Result<Unit>>;
