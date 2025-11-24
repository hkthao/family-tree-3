namespace backend.Application.Memories.DTOs;

public class CreateMemoryDto
{
    public Guid MemberId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public Guid? PhotoAnalysisId { get; set; }
    public string? PhotoUrl { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    // CreatedBy is typically handled by the application layer based on authenticated user
}
