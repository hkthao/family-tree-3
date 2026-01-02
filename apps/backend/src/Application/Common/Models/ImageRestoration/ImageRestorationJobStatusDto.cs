using backend.Domain.Enums; // Assuming RestorationStatus enum is defined here or similar

namespace backend.Application.Common.Models.ImageRestoration;

public class ImageRestorationJobStatusDto
{
    public Guid JobId { get; set; }
    public RestorationStatus Status { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string? RestoredUrl { get; set; }
    public List<string> Pipeline { get; set; } = new List<string>();
    public string? Error { get; set; }
}
