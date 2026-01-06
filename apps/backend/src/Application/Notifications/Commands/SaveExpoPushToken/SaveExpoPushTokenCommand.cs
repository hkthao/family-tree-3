using backend.Application.Common.Models;

namespace backend.Application.Notifications.Commands.SaveExpoPushToken;

public record SaveExpoPushTokenCommand : IRequest<Result>
{
    public string? UserId { get; init; }
    public string? ExpoPushToken { get; init; }
}
