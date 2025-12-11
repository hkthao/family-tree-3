using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedias.Queries.SearchFamilyMedia;

public record SearchFamilyMediaQuery() : PaginatedQuery, IRequest<Result<PaginatedList<FamilyMediaDto>>>
{
    public string? SearchQuery { get; init; }
    public Guid FamilyId { get; private set; } // Filter by linked entity ID
    public Guid? RefId { get; init; } // Filter by linked entity ID
    public RefType? RefType { get; init; } // Filter by linked entity type
    public MediaType? MediaType { get; init; } // Filter by media type

    public void SetFamilyId(Guid familyId)
    {
        FamilyId = familyId;
    }
}
