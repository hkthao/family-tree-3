namespace backend.Application.VectorStore;

public class VectorQuery
{
    public float[] Vector { get; set; } = null!;
    public int TopK { get; set; }
    public Dictionary<string, string>? Filter { get; set; }
}
