using backend.Application.Common.Models;

namespace backend.Application.Notifications.Commands.DeleteExpoPushToken;

public record DeleteExpoPushTokenCommand : IRequest<Result>
{
    public string? UserId { get; init; }
    public string? ExpoPushToken { get; init; }
}
