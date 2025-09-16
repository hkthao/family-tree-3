using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Application.Families;

public class FamilyDto : IMapFrom<Family>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Logo { get; set; }
    public string? History { get; set; }
}
