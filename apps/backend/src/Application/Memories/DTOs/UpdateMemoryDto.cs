namespace backend.Application.Memories.DTOs;

public class UpdateMemoryDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; } // MemberId should not change during update, but included for full DTO
    public string Title { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public Guid? PhotoAnalysisId { get; set; }
    public string? PhotoUrl { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    // UpdatedBy is typically handled by the application layer based on authenticated user
}
