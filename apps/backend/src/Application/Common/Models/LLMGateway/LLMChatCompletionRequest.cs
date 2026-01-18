using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.LLMGateway;

public class LLMChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("messages")]
    public List<LLMMessage> Messages { get; set; } = new List<LLMMessage>();

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.0f;

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; } = 512;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}
