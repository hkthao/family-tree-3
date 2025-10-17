using backend.Application.Common.Models;
using backend.Application.Members.Inputs;

namespace backend.Application.Members.Commands.UpdateMember;

public record UpdateMemberCommand : MemberInput, IRequest<Result<Guid>>
{
    public Guid Id { get; init; }
}
