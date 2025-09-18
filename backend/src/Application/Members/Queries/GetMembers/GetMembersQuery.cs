using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Members.Queries.GetMembers;

public record GetMembersQuery : IRequest<List<MemberDto>>;
