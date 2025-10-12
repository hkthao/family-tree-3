using backend.Application.Common.Models;
using backend.Application.Members.Inputs;

namespace backend.Application.Members.Commands.CreateMember;

public record CreateMemberCommand : MemberInput, IRequest<Result<Guid>>
{
}
