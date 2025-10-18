using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;

namespace backend.Application.Members.Queries.GetEditableMembers;

public record GetEditableMembersQuery : IRequest<Result<List<MemberListDto>>>;
