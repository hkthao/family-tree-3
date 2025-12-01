using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common; // For MemberFaceDto

namespace backend.Application.MemberFaces.Queries.SearchMemberFaces;

public record SearchMemberFacesQuery : PaginatedQuery, IRequest<Result<PaginatedList<MemberFaceDto>>>
{
    public Guid? MemberId { get; init; }
    public Guid? FamilyId { get; init; }
    public string? Emotion { get; init; }
    public string? SearchQuery { get; init; }
}
