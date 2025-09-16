using backend.Domain.Enums;
using MediatR;
using MongoDB.Bson;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public record CreateRelationshipCommand : IRequest<string>
{
    public string? MemberId { get; init; }
    public RelationshipType Type { get; init; }
    public string? TargetId { get; init; }
}
