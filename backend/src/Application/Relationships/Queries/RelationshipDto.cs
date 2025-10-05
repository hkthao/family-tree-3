using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Queries;

public class RelationshipDto : IMapFrom<Relationship>
{
    public Guid Id { get; set; }
    public Guid SourceMemberId { get; set; }
    public Guid TargetMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public int? Order { get; set; }
}
