using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Queries.SearchFamilyLinks;

public record SearchFamilyLinksQuery : IRequest<Result<PaginatedList<FamilyLinkDto>>>
{
    public Guid FamilyId { get; init; }
    public string? SearchQuery { get; init; }
    public Guid? OtherFamilyId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}

