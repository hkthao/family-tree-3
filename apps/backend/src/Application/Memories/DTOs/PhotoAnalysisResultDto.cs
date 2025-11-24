using System.Text.Json;

namespace backend.Application.Memories.DTOs;

public class PhotoAnalysisResultDto
{
    public Guid Id { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Scene { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public JsonDocument? Faces { get; set; }
    public JsonDocument? Objects { get; set; }
    public string YearEstimate { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
