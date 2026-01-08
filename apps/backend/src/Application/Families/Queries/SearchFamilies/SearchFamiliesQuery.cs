using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.SearchFamilies;

public record SearchFamiliesQuery : PaginatedQuery, IRequest<Result<PaginatedList<FamilyDto>>>
{
    public string? SearchQuery { get; init; }
    public string? Visibility { get; init; }
    public bool? IsFollowing { get; init; }
}
