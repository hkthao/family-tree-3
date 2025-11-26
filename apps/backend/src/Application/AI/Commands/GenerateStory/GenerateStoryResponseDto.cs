namespace backend.Application.Memories.DTOs;

public class GenerateStoryResponseDto
{
    public string Title { get; set; } = string.Empty;
    public string DraftStory { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public List<TimelineEntryDto> Timeline { get; set; } = new List<TimelineEntryDto>();
}

public class TimelineEntryDto
{
    public int Year { get; set; }
    public string Event { get; set; } = string.Empty;
}
