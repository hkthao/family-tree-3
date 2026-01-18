using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.LLMGateway;

public class LLMEmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("input")]
    public string Input { get; set; } = null!;

    [JsonPropertyName("encoding_format")]
    public string EncodingFormat { get; set; } = "float"; // Default to "float"
}
