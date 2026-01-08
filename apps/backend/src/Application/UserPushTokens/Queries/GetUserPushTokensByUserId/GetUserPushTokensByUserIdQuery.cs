using backend.Application.Common.Models;
using backend.Application.UserPushTokens.DTOs;

namespace backend.Application.UserPushTokens.Queries.GetUserPushTokensByUserId;

public record GetUserPushTokensByUserIdQuery(Guid UserId) : IRequest<Result<List<UserPushTokenDto>>>;
