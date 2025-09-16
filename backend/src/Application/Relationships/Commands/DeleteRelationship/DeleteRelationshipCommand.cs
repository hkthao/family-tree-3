using MediatR;
using MongoDB.Bson;

namespace backend.Application.Relationships.Commands.DeleteRelationship;

public record DeleteRelationshipCommand(string Id) : IRequest;
