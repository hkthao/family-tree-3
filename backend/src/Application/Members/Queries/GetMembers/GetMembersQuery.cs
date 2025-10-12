using backend.Application.Common.Models; // Added

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersQuery : IRequest<Result<IReadOnlyList<MemberListDto>>>
{
    public string? SearchTerm { get; init; }
    public Guid? FamilyId { get; init; }
}
