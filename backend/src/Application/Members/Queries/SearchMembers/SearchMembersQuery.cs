using backend.Application.Common.Models;
namespace backend.Application.Members.Queries.SearchMembers;

public record SearchMembersQuery : IRequest<PaginatedList<MemberDto>>
{
    public string? Keyword { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
