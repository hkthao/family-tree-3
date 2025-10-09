using backend.Application.Common.Models;
using backend.Application.UserProfiles.Queries;

namespace backend.Application.UserProfiles.Queries.GetUserProfileById;

public record GetUserProfileByAuth0IdQuery : IRequest<Result<UserProfileDto>>
{
    public string Id { get; init; } = null!;
}
