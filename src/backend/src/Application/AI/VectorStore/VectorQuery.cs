namespace backend.Application.AI.VectorStore;

public class VectorQuery
{
    public double[] Vector { get; set; } = null!;
    public int TopK { get; set; }
    public Dictionary<string, string>? Filter { get; set; }
}
