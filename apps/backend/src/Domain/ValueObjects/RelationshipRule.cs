using backend.Domain.Entities; // Required for Member
using System;
using System.Collections.Generic;

namespace backend.Domain.ValueObjects;

// Represents a rule for inferring a Vietnamese relationship based on a detected pattern and additional conditions.
public record RelationshipRule(RelationshipPattern Pattern, Func<RelationshipPath, IReadOnlyDictionary<Guid, Member>, bool> Condition, string VietnameseRelationship);
