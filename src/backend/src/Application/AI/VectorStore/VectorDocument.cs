namespace backend.Application.AI.VectorStore;

public class VectorDocument
{
    public string Id { get; set; } = null!;
    public string Content { get; set; } = null!;
    public double[] Vector { get; set; } = null!;
    public Dictionary<string, string>? Metadata { get; set; }
}
