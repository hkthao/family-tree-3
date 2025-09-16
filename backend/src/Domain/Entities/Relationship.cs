
namespace backend.Domain.Entities;

public class Relationship : BaseAuditableEntity
{
    public int MemberId { get; set; }
    public RelationshipType Type { get; set; }
    public int TargetId { get; set; }
}
