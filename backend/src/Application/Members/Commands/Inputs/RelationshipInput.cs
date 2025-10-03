using backend.Domain.Enums;

namespace backend.Application.Members.Inputs;

public class RelationshipInput
{
    public Guid? Id { get; set; } // Null for new relationships
    public Guid TargetMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public int? Order { get; set; }
}