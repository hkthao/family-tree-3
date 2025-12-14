using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Queries.SearchFamilyLinks;

public record SearchFamilyLinksQuery : PaginatedQuery, IRequest<Result<PaginatedList<FamilyLinkDto>>>
{
    public Guid FamilyId { get; init; }
    public string? SearchQuery { get; init; }
    public Guid? OtherFamilyId { get; init; }
}

