using backend.Domain.Enums;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public record CreateRelationshipCommand : IRequest<Guid>
{
    public Guid? MemberId { get; init; }
    public RelationshipType Type { get; init; }
    public Guid? TargetId { get; init; }
    public Guid? FamilyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
