using backend.Application.Common.Models;

namespace backend.Application.Events.Commands.SendEventNotification;

public record SendEventNotificationCommand : IRequest<Result<string>>
{
    public Guid EventId { get; init; }
}
