using backend.Application.Common.Models;

namespace backend.Application.Members.Queries.GetMembers;

public record class GetMembersWithPaginationQuery : PaginatedQuery, IRequest<PaginatedList<MemberListDto>>
{
    public string? SearchTerm { get; init; }
    public Guid? FamilyId { get; init; }
}