using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.SearchFamilies;

public record SearchFamiliesQuery : PaginatedQuery, IRequest<PaginatedList<FamilyDto>>
{
    public string? SearchQuery { get; init; }
}
