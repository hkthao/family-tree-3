using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyDicts.Queries;

public record SearchFamilyDictsQuery : IRequest<Result<PaginatedList<FamilyDictDto>>>
{
    public string? Q { get; init; }
    public FamilyDictLineage? Lineage { get; init; }
    public string? Region { get; init; } // "north", "central", "south"
    public int Page { get; init; } = 1;
    public int ItemsPerPage { get; init; } = 10;
}
