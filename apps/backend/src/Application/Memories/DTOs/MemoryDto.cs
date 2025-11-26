using backend.Application.AI.DTOs; // NEW USING

namespace backend.Application.Memories.DTOs;

public class MemoryDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public Guid? PhotoAnalysisId { get; set; }
    public string? PhotoUrl { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
    public PhotoAnalysisResultDto? PhotoAnalysisResult { get; set; }
}
