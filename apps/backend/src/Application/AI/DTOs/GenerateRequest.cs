namespace backend.Application.AI.DTOs;

public class GenerateRequest : ChatRequest
{
    public new string SystemPrompt { get; set; } = string.Empty;
}
