using backend.Application.Common.Models;

namespace backend.Application.Identity.UserProfiles.Queries.GetUserProfileByUserId;

public record GetUserProfileByUserIdQuery : IRequest<Result<UserProfileDto>>
{
    public Guid UserId { get; init; }
}
