
using MongoDB.Bson;

namespace backend.Domain.Entities;

public class Relationship : BaseAuditableEntity
{
    public ObjectId FamilyId { get; set; }
    public ObjectId SourceMemberId { get; set; }
    public ObjectId TargetMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
