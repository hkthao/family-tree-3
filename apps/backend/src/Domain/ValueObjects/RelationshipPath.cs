namespace backend.Domain.ValueObjects;

public record RelationshipPath
{
    public List<Guid> NodeIds { get; init; } = new List<Guid>();
    public List<GraphEdge> Edges { get; init; } = new List<GraphEdge>();

    public RelationshipPath() { }

    public RelationshipPath(List<Guid> nodeIds, List<GraphEdge> edges)
    {
        NodeIds = nodeIds;
        Edges = edges;
    }
}
