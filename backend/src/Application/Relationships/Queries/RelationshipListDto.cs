using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Queries;

public class RelationshipListDto : IMapFrom<Relationship>
{
    public Guid Id { get; set; }
    public Guid SourceMemberId { get; set; }
    public Guid TargetMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public int? Order { get; set; }

    // Additional properties for display, e.g., member names
    public string? SourceMemberFullName { get; set; }
    public string? TargetMemberFullName { get; set; }
}
