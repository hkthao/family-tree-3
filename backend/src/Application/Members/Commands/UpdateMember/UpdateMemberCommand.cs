using backend.Application.Members.Inputs;

namespace backend.Application.Members.Commands.UpdateMember;

public record UpdateMemberCommand : MemberInput, IRequest
{
    public Guid Id { get; init; }
    public ICollection<Guid> DeletedRelationshipIds { get; init; } = [];
}
