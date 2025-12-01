namespace backend.Application.MemberStories.DTOs; // Updated

public class GenerateStoryRequestDto
{
    public Guid MemberId { get; set; }

    public string RawText { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty; // e.g., nostalgic|warm|formal|folk
    public int MaxWords { get; set; } = 500;
}
