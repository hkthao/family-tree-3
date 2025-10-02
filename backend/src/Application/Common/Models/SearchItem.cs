namespace backend.Application.Common.Models;

public class SearchItem
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!; // e.g., "Family", "Member"
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Url { get; set; }
}