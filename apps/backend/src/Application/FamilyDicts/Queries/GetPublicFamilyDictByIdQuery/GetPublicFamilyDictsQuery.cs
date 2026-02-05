using backend.Application.Common.Models;
using backend.Domain.Enums; // For FamilyDictLineage

namespace backend.Application.FamilyDicts.Queries.Public;

public record GetPublicFamilyDictsQuery : IRequest<Result<PaginatedList<FamilyDictDto>>>
{
    public int Page { get; init; } = 1;
    public int ItemsPerPage { get; init; } = 10;
    public string? SearchQuery { get; init; }
    public FamilyDictLineage? Lineage { get; init; }
    public string? Region { get; init; } // e.g., North, Central, South
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; } // "asc" or "desc"
}
