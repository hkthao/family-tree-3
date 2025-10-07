using backend.Application.Members.Inputs;
using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.CreateMember;

public record CreateMemberCommand : MemberInput, IRequest<Result<Guid>>
{
}