using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Enums;

namespace backend.Application.Members.Queries.SearchPublicMembers;

public record SearchPublicMembersQuery : PaginatedQuery, IRequest<Result<PaginatedList<MemberListDto>>>
{
    public Guid FamilyId { get; init; }
    public string? SearchTerm { get; init; }
    public Gender? Gender { get; init; }
    public bool? IsRoot { get; init; }
}
