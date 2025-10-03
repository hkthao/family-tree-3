using backend.Application.Common.Models;

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersWithPaginationQuery : IRequest<PaginatedList<MemberListDto>>
{
    public string? SearchTerm { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}