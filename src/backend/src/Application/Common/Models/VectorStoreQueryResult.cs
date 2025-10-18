namespace backend.Application.Common.Models;

public class VectorStoreQueryResult
{
    public string Id { get; set; } = null!;
    public List<float> Embedding { get; set; } = new List<float>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public float Score { get; set; }
    public string Content { get; set; } = string.Empty;
}
