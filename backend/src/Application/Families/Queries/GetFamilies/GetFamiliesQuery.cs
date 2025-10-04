using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.GetFamilies;

public record class GetFamiliesQuery : PaginatedQuery, IRequest<IReadOnlyList<FamilyListDto>>
{
    public string? SearchTerm { get; init; }
}