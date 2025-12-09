using backend.Domain.ValueObjects;
using backend.Domain.Entities; // Required for Member
using System.Collections.Generic;

namespace backend.Domain.Interfaces;

public interface IRelationshipRuleEngine
{
    string InferRelationship(RelationshipPath path, IReadOnlyDictionary<Guid, Member> allMembers);
}
