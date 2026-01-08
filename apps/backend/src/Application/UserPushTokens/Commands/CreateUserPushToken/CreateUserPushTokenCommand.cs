using backend.Application.Common.Models;

namespace backend.Application.UserPushTokens.Commands.CreateUserPushToken;

public record CreateUserPushTokenCommand : CreateUserPushTokenInput, IRequest<Result<Guid>>;
