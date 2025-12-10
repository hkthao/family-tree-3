using backend.Application.Common.Models;
using backend.Application.FamilyMedia.DTOs;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedia.Queries.GetFamilyMediaList;

public record FamilyMediaFilter
{
    public string? SearchQuery { get; init; }
    public Guid? RefId { get; init; } // Filter by linked entity ID
    public RefType? RefType { get; init; } // Filter by linked entity type
    public MediaType? MediaType { get; init; } // Filter by media type
}

public record GetFamilyMediaListQuery(Guid FamilyId, FamilyMediaFilter? Filters, int PageNumber = 1, int PageSize = 10, string? OrderBy = null) : IRequest<Result<PaginatedList<FamilyMediaDto>>>;
