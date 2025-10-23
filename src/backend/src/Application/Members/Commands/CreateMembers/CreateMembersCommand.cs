using backend.Application.Common.Models;
using backend.Application.Members.Queries;

namespace backend.Application.Members.Commands.CreateMembers;

public record CreateMembersCommand(List<AIMemberDto> Members) : IRequest<Result<List<Guid>>>;
