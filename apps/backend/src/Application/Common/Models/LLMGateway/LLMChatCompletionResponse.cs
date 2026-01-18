using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.LLMGateway;

public class LLMChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("object")]
    public string Object { get; set; } = null!;

    [JsonPropertyName("choices")]
    public List<LLMChatCompletionChoice> Choices { get; set; } = new List<LLMChatCompletionChoice>();
}
