using backend.Application.Common.Models;
using backend.Application.UserProfiles.Queries;

namespace backend.Application.UserProfiles.Queries.GetUserProfileByAuth0UserId;

public record GetUserProfileByAuth0UserIdQuery : IRequest<Result<UserProfileDto>>
{
    public string Auth0UserId { get; init; } = null!;
}
