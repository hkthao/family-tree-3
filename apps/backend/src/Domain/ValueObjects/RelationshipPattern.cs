using backend.Domain.Enums;

namespace backend.Domain.ValueObjects;

// Represents a sequence of relationship types that form a pattern for rule matching.
// Example: [RelationshipType.Child, RelationshipType.Father, RelationshipType.Father] for "ông nội"
public record RelationshipPattern(IReadOnlyList<RelationshipType> PatternTypes);
