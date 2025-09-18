using backend.Application.Members;
using MediatR;

namespace backend.Application.Members.Queries.GetMemberById;

public record GetMemberByIdQuery(string Id) : IRequest<MemberDto>;
