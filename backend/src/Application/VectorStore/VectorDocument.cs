namespace backend.Application.VectorStore;

public class VectorDocument
{
    public string Id { get; set; } = null!;
    public string Content { get; set; } = null!;
    public float[] Vector { get; set; } = null!;
    public Dictionary<string, string>? Metadata { get; set; }
}
