using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.SearchFamilies;

public record SearchFamiliesQuery : IRequest<PaginatedList<FamilyDto>>
{
    public string? Keyword { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
