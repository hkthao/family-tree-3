namespace backend.Application.Memories.DTOs;

public class GenerateStoryRequestDto
{
    public Guid MemberId { get; set; }
    public Guid? PhotoAnalysisId { get; set; }
    public string RawText { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty; // e.g., nostalgic|warm|formal|folk
    public int MaxWords { get; set; } = 500;
}
