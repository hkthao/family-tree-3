namespace backend.Application.AI.DTOs;

public class ChatResponse
{
    public string? Output { get; set; }
    public CombinedAiContentDto? GeneratedData { get; set; }
    public string? Intent { get; set; } // New property for redirection URL
}
