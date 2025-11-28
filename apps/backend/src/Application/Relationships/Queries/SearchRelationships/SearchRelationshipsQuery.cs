using backend.Application.Common.Models;

namespace backend.Application.Relationships.Queries.SearchRelationships;

public record SearchRelationshipsQuery : PaginatedQuery, IRequest<Result<PaginatedList<RelationshipListDto>>>
{
    public string? SearchQuery { get; init; }
    public Guid? SourceMemberId { get; init; }
    public Guid? TargetMemberId { get; init; }
    public string? Type { get; init; }
}
