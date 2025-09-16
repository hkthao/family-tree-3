using backend.Application.Members;
using MediatR;
using MongoDB.Bson;

namespace backend.Application.Members.Queries.GetMemberById;

public record GetMemberByIdQuery(string Id) : IRequest<MemberDto>;
