using backend.Application.Common.Models;

namespace backend.Application.UserPushTokens.Commands.SyncUserPushToken;

public record SyncCurrentUserPushTokenCommand(string ExpoPushToken, string Platform, string DeviceId) : IRequest<Result>;
