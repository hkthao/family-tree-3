using backend.Domain.Enums;

namespace backend.Application.Relationships.Commands.Inputs;

public abstract record RelationshipInput
{
    public Guid SourceMemberId { get; init; }
    public Guid TargetMemberId { get; init; }
    public RelationshipType Type { get; init; }
    public int? Order { get; init; }
    public DateTime? StartDate { get; init; } // Added
    public DateTime? EndDate { get; init; } // Added
    public string? Description { get; init; } // Added
}
