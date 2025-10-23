using backend.Domain.Enums;

namespace backend.Application.Relationships.Commands.Inputs;

public abstract record RelationshipInput
{
    public Guid SourceMemberId { get; set; }
    public Guid TargetMemberId { get; set; }
    public RelationshipType Type { get; set; }
    public int? Order { get; set; }
}
