using backend.Application.Common.Models;

namespace backend.Application.UserProfiles.Queries.GetUserProfileById;

public record GetUserProfileByIdQuery : IRequest<Result<UserProfileDto>>
{
    public Guid Id { get; init; }
}
