using backend.Application.Common.Models;

namespace backend.Application.UserPushTokens.Commands.UpdateUserPushToken;

public record UpdateUserPushTokenCommand : UpdateUserPushTokenInput, IRequest<Result<Guid>>
{
    public Guid Id { get; init; }
}
