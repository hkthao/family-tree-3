using System.Collections.Generic;
using backend.Domain.Entities; // Required for Member
using backend.Domain.ValueObjects;

namespace backend.Domain.Interfaces;

public interface IRelationshipRuleEngine
{
    string InferRelationship(RelationshipPath path, IReadOnlyDictionary<Guid, Member> allMembers);
}
