using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.LLMGateway;

public class LLMChatCompletionChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public LLMChatCompletionMessage Message { get; set; } = null!;

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = null!;
}
