using backend.Domain.Enums;

namespace backend.Application.Relationships.Commands.UpdateRelationship;

public record UpdateRelationshipCommand : IRequest
{
    public Guid Id { get; init; } 
    public Guid? SourceMemberId { get; init; }
    public Guid? TargetMemberId { get; init; }
    public RelationshipType Type { get; init; }
    public Guid? FamilyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
