using backend.Domain.Enums; // Assuming RestorationStatus enum is defined here or similar

namespace backend.Application.Common.Models.ImageRestoration;

public class StartImageRestorationResponseDto
{
    public Guid JobId { get; set; }
    public RestorationStatus Status { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string? Error { get; set; }
}
