using backend.Application.Common.Models;

namespace backend.Application.Relationships.Queries.GetRelationships
{
    public record GetRelationshipsQuery : PaginatedQuery, IRequest<Result<PaginatedList<RelationshipListDto>>>
    {
        public Guid? FamilyId { get; init; }
        public Guid? SourceMemberId { get; init; }
        public Guid? TargetMemberId { get; init; }
        public string? Type { get; init; }
    }
}
