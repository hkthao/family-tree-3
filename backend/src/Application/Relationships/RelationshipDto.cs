using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Relationships;

public class RelationshipDto : IMapFrom<Relationship>
{
    public string? Id { get; set; }
    public string? FamilyId { get; set; }
    public string? SourceMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public string? TargetMemberId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
