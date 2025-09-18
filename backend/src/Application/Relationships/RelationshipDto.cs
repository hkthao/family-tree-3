using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Relationships;

public class RelationshipDto : IMapFrom<Relationship>
{
    public Guid? Id { get; set; }
    public Guid? FamilyId { get; set; }
    public Guid? SourceMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public Guid? TargetMemberId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Relationship, RelationshipDto>();
    }
}
