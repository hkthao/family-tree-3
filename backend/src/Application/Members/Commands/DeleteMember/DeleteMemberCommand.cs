namespace backend.Application.Members.Commands.DeleteMember;

public record DeleteMemberCommand(Guid Id) : IRequest;
