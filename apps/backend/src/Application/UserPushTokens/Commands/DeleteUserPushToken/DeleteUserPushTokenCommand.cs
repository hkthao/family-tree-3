using backend.Application.Common.Models;

namespace backend.Application.UserPushTokens.Commands.DeleteUserPushToken;

public record DeleteUserPushTokenCommand(Guid Id) : IRequest<Result>;
