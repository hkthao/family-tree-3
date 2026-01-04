using System.Text.Json.Serialization; // Added for JsonPropertyName
using backend.Domain.Enums; // Assuming RestorationStatus enum is defined here or similar

namespace backend.Application.Common.Models.ImageRestoration;

public class StartImageRestorationResponseDto
{
    [JsonPropertyName("job_id")]
    public Guid JobId { get; set; }
    [JsonPropertyName("status")]
    public RestorationStatus Status { get; set; }
    [JsonPropertyName("original_url")]
    public string OriginalUrl { get; set; } = string.Empty;
    [JsonPropertyName("restored_url")]
    public string? RestoredUrl { get; set; } // Added
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
