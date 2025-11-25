namespace backend.Application.Memories.DTOs;

public class OpenAIAnalysisResponseDto
{
    public string AdvancedEmotion { get; set; } = string.Empty;
    public string ContextDescription { get; set; } = string.Empty;
    public string SuggestedStory { get; set; } = string.Empty;
    public string Scene { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string YearEstimate { get; set; } = string.Empty;
}
