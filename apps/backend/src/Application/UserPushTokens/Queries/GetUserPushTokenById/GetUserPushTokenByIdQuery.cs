using backend.Application.Common.Models;
using backend.Application.UserPushTokens.DTOs;

namespace backend.Application.UserPushTokens.Queries.GetUserPushTokenById;

public record GetUserPushTokenByIdQuery(Guid Id) : IRequest<Result<UserPushTokenDto>>;
