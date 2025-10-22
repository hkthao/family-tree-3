namespace backend.Application.AI.VectorStore;

public class VectorStoreQueryResult
{
    public string Id { get; set; } = null!;
    public List<double> Embedding { get; set; } = [];
    public Dictionary<string, string> Metadata { get; set; } = [];
    public double Score { get; set; }
    public string Content { get; set; } = string.Empty;
}
