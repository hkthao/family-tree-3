namespace backend.Application.AI.DTOs;

public class GenerateRequest: ChatRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
}
