using backend.Application.Common.Models;
using backend.Application.FamilyMedia.DTOs;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedia.Queries.SearchFamilyMedia;

public record SearchFamilyMediaQuery(Guid FamilyId) : PaginatedQuery, IRequest<Result<PaginatedList<FamilyMediaDto>>>
{
    public string? SearchQuery { get; init; }
    public Guid? RefId { get; init; } // Filter by linked entity ID
    public RefType? RefType { get; init; } // Filter by linked entity type
    public MediaType? MediaType { get; init; } // Filter by media type
}
