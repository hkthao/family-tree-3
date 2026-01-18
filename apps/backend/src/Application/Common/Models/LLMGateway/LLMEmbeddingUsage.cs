using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.LLMGateway;

public class LLMEmbeddingUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; } = 0;

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; } = 0;
}
