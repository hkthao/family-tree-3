using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.LLMGateway;

public class LLMChatCompletionMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = null!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;
}
