using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.SearchPublicFamilies;

public record SearchPublicFamiliesQuery : PaginatedQuery, IRequest<Result<PaginatedList<FamilyDto>>>
{
    public string? SearchQuery { get; init; }
}
