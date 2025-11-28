namespace backend.Application.MemberStories.DTOs; // Updated

public class MemberStoryDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
}
