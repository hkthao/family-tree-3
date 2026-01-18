using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.LLMGateway;

public class LLMEmbeddingResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "list";

    [JsonPropertyName("data")]
    public List<LLMEmbeddingData> Data { get; set; } = new List<LLMEmbeddingData>();

    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("usage")]
    public LLMEmbeddingUsage Usage { get; set; } = new LLMEmbeddingUsage();
}
