using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;

namespace backend.Application.Members.Queries.SearchMembers;

public record SearchMembersQuery : PaginatedQuery, IRequest<PaginatedList<MemberListDto>>
{
    public string? SearchQuery { get; init; }
}
