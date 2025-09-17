using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Domain.Entities;

public class Family : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
}
