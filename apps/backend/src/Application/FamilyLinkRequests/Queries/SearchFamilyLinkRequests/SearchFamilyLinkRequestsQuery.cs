using backend.Application.Common.Models;
using backend.Application.FamilyLinks.Queries; // New using directive

namespace backend.Application.FamilyLinkRequests.Queries.SearchFamilyLinkRequests;

public record SearchFamilyLinkRequestsQuery(Guid FamilyId) : PaginatedQuery, IRequest<Result<PaginatedList<FamilyLinkRequestDto>>>
{
    public string? SearchQuery { get; init; }
    public string? Status { get; init; }
    public Guid? OtherFamilyId { get; init; }
}