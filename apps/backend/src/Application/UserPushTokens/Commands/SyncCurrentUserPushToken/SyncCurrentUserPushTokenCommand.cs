using backend.Application.Common.Models;

namespace backend.Application.UserPushTokens.Commands.SyncCurrentUserPushToken;

public record SyncCurrentUserPushTokenCommand(string ExpoPushToken, string Platform, string DeviceId) : IRequest<Result<Guid>>;
