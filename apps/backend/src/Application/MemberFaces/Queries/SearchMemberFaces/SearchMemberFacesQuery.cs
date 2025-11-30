using backend.Application.Common.Models;
using MediatR;
using backend.Application.MemberFaces.Queries.MemberFaces; // For MemberFaceDto

namespace backend.Application.MemberFaces.Queries.SearchMemberFaces;

public record SearchMemberFacesQuery : PaginatedQuery, IRequest<Result<PaginatedList<MemberFaceDto>>>
{
    public Guid? MemberId { get; init; }
    public Guid? FamilyId { get; init; }
}
