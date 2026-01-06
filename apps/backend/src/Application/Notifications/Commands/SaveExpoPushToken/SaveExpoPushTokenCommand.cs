using backend.Application.Common.Models;

namespace backend.Application.Notifications.Commands.SaveExpoPushToken;

public record SaveExpoPushTokenCommand(string? UserId) : IRequest<Result>
{
}
