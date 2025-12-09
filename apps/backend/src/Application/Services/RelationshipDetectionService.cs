using backend.Application.Common.Interfaces; // Correct namespace for IApplicationDbContext
using backend.Domain.Interfaces;

namespace backend.Application.Services;

public class RelationshipDetectionService : IRelationshipDetectionService
{
    private readonly IApplicationDbContext _context;
    private readonly IRelationshipGraph _relationshipGraph;
    private readonly IRelationshipRuleEngine _ruleEngine;

    public RelationshipDetectionService(IApplicationDbContext context, IRelationshipGraph relationshipGraph, IRelationshipRuleEngine ruleEngine)
    {
        _context = context;
        _relationshipGraph = relationshipGraph;
        _ruleEngine = ruleEngine;
    }

    public async Task<RelationshipDetectionResult> DetectRelationshipAsync(Guid familyId, Guid memberAId, Guid memberBId)
    {
        // 1. Fetch all Members and Relationships for the given FamilyId
        var members = await _context.Members
                                    .Where(m => m.FamilyId == familyId)
                                    .ToListAsync();

        var relationships = await _context.Relationships
                                        .Where(r => r.FamilyId == familyId)
                                        .ToListAsync();

        // Create a dictionary for quick member lookup
        var allMembers = members.ToDictionary(m => m.Id);

        // 2. Build the graph
        _relationshipGraph.BuildGraph(members, relationships);

        // 3. Find shortest path from A to B
        var pathToB = _relationshipGraph.FindShortestPath(memberAId, memberBId);

        // 4. Infer relationship from A to B
        string fromAToB = "unknown";
        if (pathToB.NodeIds.Any())
        {
            fromAToB = _ruleEngine.InferRelationship(pathToB, allMembers);
        }

        // 5. Find shortest path from B to A
        // Note: For bidirectional relationships, we might consider finding the path directly from B to A
        // or reversing the A to B path and inferring. For now, let's find it directly.
        var pathToA = _relationshipGraph.FindShortestPath(memberBId, memberAId);

        // 6. Infer relationship from B to A
        string fromBToA = "unknown";
        if (pathToA.NodeIds.Any())
        {
            fromBToA = _ruleEngine.InferRelationship(pathToA, allMembers);
        }

        // 7. Map GraphEdge Type enum to string for output
        var edgesToString = pathToB.Edges.Select(e => e.Type.ToString()).ToList();

        // Return the result
        return new RelationshipDetectionResult
        {
            FromAToB = fromAToB,
            FromBToA = fromBToA,
            Path = pathToB.NodeIds,
            Edges = edgesToString
        };
    }
}
