using MediatR;
using MongoDB.Bson;

namespace backend.Application.Members.Commands.DeleteMember;

public record DeleteMemberCommand(string Id) : IRequest;
