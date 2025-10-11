using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;

namespace backend.Application.Members.Queries.SearchMembers
{
    public record SearchMembersQuery : PaginatedQuery, IRequest<Result<PaginatedList<MemberListDto>>>
    {
        public string? SearchQuery { get; init; }
        public string? Gender { get; init; }
        public Guid? FamilyId { get; init; }
    }
}
