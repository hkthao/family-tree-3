using backend.Application.Common.Models;

namespace backend.Application.UserPushTokens.Commands.RemoveUserPushToken;

public record RemoveUserPushTokenCommand(string DeviceId, string ExpoPushToken, Guid UserId) : IRequest<Result>;
