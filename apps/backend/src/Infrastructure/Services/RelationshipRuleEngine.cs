using backend.Domain.Enums;
using backend.Domain.Interfaces;
using backend.Domain.ValueObjects;
using backend.Domain.Entities; // Added
using System.Collections.Generic;
using System.Linq;

namespace backend.Infrastructure.Services;

public class RelationshipRuleEngine : IRelationshipRuleEngine
{
    private readonly List<RelationshipRule> _rules;

    public RelationshipRuleEngine()
    {
        _rules = RuleDefinitions.GetRules();
    }

    public string InferRelationship(RelationshipPath path, IReadOnlyDictionary<Guid, Member> allMembers)
    {
        if (!path.Edges.Any())
        {
            return "Chính bản thân"; // A to A relationship
        }

        var pathPatternTypes = path.Edges.Select(e => e.Type).ToList();

        foreach (var rule in _rules)
        {
            if (MatchPattern(pathPatternTypes, rule.Pattern.PatternTypes) && rule.Condition(path, allMembers))
            {
                return rule.VietnameseRelationship;
            }
        }

        return "unknown";
    }

    private bool MatchPattern(IReadOnlyList<RelationshipType> pathTypes, IReadOnlyList<RelationshipType> ruleTypes)
    {
        if (pathTypes.Count != ruleTypes.Count)
        {
            return false;
        }

        for (int i = 0; i < pathTypes.Count; i++)
        {
            if (pathTypes[i] != ruleTypes[i])
            {
                return false;
            }
        }
        return true;
    }
}
