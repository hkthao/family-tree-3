using backend.Application.Common.Models;

namespace backend.Application.UserPushTokens.Commands.RemoveCurrentUserPushToken;

public record RemoveCurrentUserPushTokenCommand(string DeviceId, string ExpoPushToken) : IRequest<Result>;
