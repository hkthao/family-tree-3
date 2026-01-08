using backend.Application.Common.Models;

namespace backend.Application.FamilyFollows.Queries.GetFamilyFollowers;

public record GetFamilyFollowersQuery : IRequest<Result<ICollection<FamilyFollowDto>>>
{
    public Guid FamilyId { get; init; }
}
