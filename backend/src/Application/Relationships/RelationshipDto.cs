using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Application.Relationships;

public class RelationshipDto : IMapFrom<Relationship>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? MemberId { get; set; }
    public RelationshipType Type { get; set; }
    public string? TargetId { get; set; }
}
