using backend.Domain.Enums;
using backend.Domain.Events.Relationships;

namespace backend.Domain.Entities;

public class Relationship : BaseAuditableEntity
{
    public Guid SourceMemberId { get; private set; }
    public Member SourceMember { get; private set; } = null!;
    public Guid TargetMemberId { get; private set; }
    public Member TargetMember { get; private set; } = null!;
    public RelationshipType Type { get; private set; }
    public int? Order { get; private set; }
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;

    // Private constructor for EF Core
    private Relationship() { }

    public Relationship(Guid familyId, Guid sourceMemberId, Guid targetMemberId, RelationshipType type, int? order = null)
    {
        FamilyId = familyId;
        SourceMemberId = sourceMemberId;
        TargetMemberId = targetMemberId;
        Type = type;
        Order = order;
    }

    public void Update(Guid sourceMemberId, Guid targetMemberId, RelationshipType type, int? order)
    {
        SourceMemberId = sourceMemberId;
        TargetMemberId = targetMemberId;
        Type = type;
        Order = order;
        AddDomainEvent(new RelationshipUpdatedEvent(this));
    }

    public Relationship(Guid familyId)
    {
        FamilyId = familyId;
    }
}
