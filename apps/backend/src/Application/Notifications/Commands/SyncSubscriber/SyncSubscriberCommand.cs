using backend.Application.Common.Models;

namespace backend.Application.Notifications.Commands.SyncSubscriber;

public record SyncSubscriberCommand : IRequest<Result>
{
    public string? UserId { get; init; }
}
