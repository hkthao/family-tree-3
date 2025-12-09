using backend.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace backend.Domain.ValueObjects;

// Represents a sequence of relationship types that form a pattern for rule matching.
// Example: [RelationshipType.Child, RelationshipType.Father, RelationshipType.Father] for "ông nội"
public record RelationshipPattern(IReadOnlyList<RelationshipType> PatternTypes);
