using backend.Application.Common.Models;

namespace backend.Application.UserProfiles.Queries.GetUserProfileById;

public record GetUserProfileByAuth0IdQuery : IRequest<Result<UserProfileDto>>
{
    public string Auth0UserId { get; init; } = null!;
}
