using backend.Application.Common.Models;
using MediatR;
using MongoDB.Bson;

namespace backend.Application.Members.Queries.GetMembers;

public record GetMembersQuery : IRequest<List<MemberDto>>;
