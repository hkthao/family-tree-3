
using MongoDB.Bson;

namespace backend.Domain.Entities;

public class Relationship : BaseAuditableEntity
{
    public ObjectId MemberId { get; set; }
    public RelationshipType Type { get; set; }
    public ObjectId TargetId { get; set; }
}
