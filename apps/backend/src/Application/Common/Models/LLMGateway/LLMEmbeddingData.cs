using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.LLMGateway;

public class LLMEmbeddingData
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "embedding";

    [JsonPropertyName("embedding")]
    public List<float> Embedding { get; set; } = new List<float>();

    [JsonPropertyName("index")]
    public int Index { get; set; } = 0;
}
