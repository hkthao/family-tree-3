using backend.Application.Common.Models;

namespace backend.Application.FamilyFollows.Queries.GetUserFollowedFamilies;

public record GetUserFollowedFamiliesQuery : IRequest<Result<ICollection<FamilyFollowDto>>>
{
    // No specific parameters needed as it uses the current user's ID
}
