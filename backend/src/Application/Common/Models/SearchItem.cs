namespace backend.Application.Common.Models;

public class SearchItem
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!; // e.g., "Family", "Member"
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Url { get; set; }
}