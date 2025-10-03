namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersQuery : IRequest<IReadOnlyList<MemberListDto>>
{
    public string? SearchTerm { get; init; }
    public Guid? FamilyId { get; init; }
}