using backend.Domain.Enums;

namespace backend.Application.Relationships.Commands.Inputs
{
    public abstract record RelationshipInput
    {
        public Guid SourceMemberId { get; init; }
        public Guid TargetMemberId { get; init; }
        public RelationshipType Type { get; init; }
        public int? Order { get; init; }
    }
}
