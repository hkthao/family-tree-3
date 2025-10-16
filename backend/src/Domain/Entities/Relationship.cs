using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class Relationship : BaseEntity
{
    public Guid SourceMemberId { get; set; }
    public Member SourceMember { get; set; } = null!;

    public Guid TargetMemberId { get; set; }
    public Member TargetMember { get; set; } = null!;

    public RelationshipType Type { get; set; }
    public int? Order { get; set; } // For birth order, etc.
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}
