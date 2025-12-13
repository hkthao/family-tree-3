using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Interfaces;
using backend.Domain.ValueObjects;

namespace backend.Infrastructure.Services;

public class RelationshipGraph : IRelationshipGraph
{
    private readonly Dictionary<Guid, List<GraphEdge>> _adjacencyList = new();

    public void BuildGraph(IEnumerable<Member> members, IEnumerable<Relationship> relationships)
    {
        _adjacencyList.Clear();

        // Initialize nodes (all members)
        foreach (var member in members)
        {
            _adjacencyList[member.Id] = new List<GraphEdge>();
        }

        // Add edges
        foreach (var relationship in relationships)
        {
            // Add forward relationship
            if (!_adjacencyList.ContainsKey(relationship.SourceMemberId))
            {
                _adjacencyList[relationship.SourceMemberId] = new List<GraphEdge>();
            }
            if (!_adjacencyList.ContainsKey(relationship.TargetMemberId))
            {
                _adjacencyList[relationship.TargetMemberId] = new List<GraphEdge>();
            }
            _adjacencyList[relationship.SourceMemberId].Add(new GraphEdge(relationship.SourceMemberId, relationship.TargetMemberId, relationship.Type));

            // Add reverse relationship to allow bidirectional traversal
            var reverseType = GetReverseRelationshipType(relationship.Type);
            _adjacencyList[relationship.TargetMemberId].Add(new GraphEdge(relationship.TargetMemberId, relationship.SourceMemberId, reverseType));
        }
    }

    public RelationshipPath FindShortestPath(Guid startMemberId, Guid endMemberId)
    {
        if (startMemberId == endMemberId)
        {
            return new RelationshipPath(new List<Guid> { startMemberId }, new List<GraphEdge>());
        }

        if (!_adjacencyList.ContainsKey(startMemberId) || !_adjacencyList.ContainsKey(endMemberId))
        {
            return new RelationshipPath(); // Start or end member not in graph
        }

        // BFS implementation
        var queue = new Queue<Guid>();
        var visited = new HashSet<Guid>();
        var parentMap = new Dictionary<Guid, Guid>(); // child -> parent
        var edgeMap = new Dictionary<Guid, GraphEdge>(); // child -> edge_from_parent_to_child

        queue.Enqueue(startMemberId);
        visited.Add(startMemberId);

        while (queue.Any())
        {
            var current = queue.Dequeue();

            if (_adjacencyList.TryGetValue(current, out var edges))
            {
                foreach (var edge in edges)
                {
                    if (!visited.Contains(edge.TargetMemberId))
                    {
                        visited.Add(edge.TargetMemberId);
                        parentMap[edge.TargetMemberId] = current;
                        edgeMap[edge.TargetMemberId] = edge; // Store the edge that leads to TargetMemberId
                        queue.Enqueue(edge.TargetMemberId);

                        if (edge.TargetMemberId == endMemberId)
                        {
                            return ReconstructPath(startMemberId, endMemberId, parentMap, edgeMap);
                        }
                    }
                }
            }
        }

        return new RelationshipPath(); // No path found
    }

    private RelationshipType GetReverseRelationshipType(RelationshipType type)
    {
        return type switch
        {
            RelationshipType.Father => RelationshipType.Child,
            RelationshipType.Mother => RelationshipType.Child,
            RelationshipType.Husband => RelationshipType.Wife,
            RelationshipType.Wife => RelationshipType.Husband,
            RelationshipType.Child => throw new ArgumentException("Child relationship is already a reverse type and should not be reversed again."),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unexpected relationship type: {type}")
        };
    }

    private RelationshipPath ReconstructPath(Guid startMemberId, Guid endMemberId, Dictionary<Guid, Guid> parentMap, Dictionary<Guid, GraphEdge> edgeMap)
    {
        var nodeIds = new LinkedList<Guid>();
        var edges = new LinkedList<GraphEdge>();

        var current = endMemberId;
        // Reconstruct path from end to start
        while (current != startMemberId && parentMap.ContainsKey(current))
        {
            nodeIds.AddFirst(current);
            var edge = edgeMap[current]; // This is the edge from parent to current
            edges.AddFirst(edge);
            current = parentMap[current];
        }
        nodeIds.AddFirst(startMemberId); // Add the start node

        return new RelationshipPath(nodeIds.ToList(), edges.ToList());
    }
}
