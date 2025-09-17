using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Application.Families;

public class FamilyDto : IMapFrom<Family> // IMapFrom will create a map from Family to FamilyDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
}
