using backend.Application.Common.Models;

namespace backend.Application.Relationships.Queries.GetRelationships;

public record GetRelationshipsQuery : IRequest<Result<IList<RelationshipListDto>>>
{
    public Guid? FamilyId { get; init; }
}
