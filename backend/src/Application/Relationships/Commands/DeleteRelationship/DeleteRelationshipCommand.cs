using MediatR;

namespace backend.Application.Relationships.Commands.DeleteRelationship;

public record DeleteRelationshipCommand(string Id) : IRequest;
