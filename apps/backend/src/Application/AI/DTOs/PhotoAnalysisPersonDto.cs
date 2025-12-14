namespace backend.Application.AI.DTOs;

public class PhotoAnalysisPersonDto
{
    public string? Id { get; set; }
    public string? MemberId { get; set; }
    public string? Name { get; set; }
    public string? Emotion { get; set; }
    public double? Confidence { get; set; }
    public string? RelationPrompt { get; set; } // NEW PROPERTY
}
