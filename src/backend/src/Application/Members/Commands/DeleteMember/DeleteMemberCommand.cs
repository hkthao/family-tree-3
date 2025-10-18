using backend.Application.Common.Models;  for Result

namespace backend.Application.Members.Commands.DeleteMember;

public record DeleteMemberCommand(Guid Id) : IRequest<Result>;
