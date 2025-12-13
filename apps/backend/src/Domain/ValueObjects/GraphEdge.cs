using backend.Domain.Enums;

namespace backend.Domain.ValueObjects;

public record GraphEdge(Guid SourceMemberId, Guid TargetMemberId, RelationshipType Type);
