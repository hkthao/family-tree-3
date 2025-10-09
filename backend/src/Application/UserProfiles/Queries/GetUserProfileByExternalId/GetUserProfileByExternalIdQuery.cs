using backend.Application.Common.Models;

namespace backend.Application.UserProfiles.Queries.GetUserProfileByExternalId;

public record GetUserProfileByExternalIdQuery : IRequest<Result<UserProfileDto>>
{
    public string ExternalId { get; init; } = null!;
}
