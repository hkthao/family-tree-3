using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.LLMGateway;

public class LLMChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("messages")]
    public List<LLMChatCompletionMessage> Messages { get; set; } = new List<LLMChatCompletionMessage>();

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.0f;

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; } = 512;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}
