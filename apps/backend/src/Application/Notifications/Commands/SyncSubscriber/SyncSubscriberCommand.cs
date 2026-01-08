using backend.Application.Common.Models;

namespace backend.Application.Notifications.Commands.SyncSubscriber;

public record SyncSubscriberCommand(Guid UserId) : IRequest<Result>;
