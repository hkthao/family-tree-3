using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.DeleteMember;

public record DeleteMemberCommand(Guid Id) : IRequest<Result>
{
    public bool SkipDomainEvent { get; init; } = false;
}
