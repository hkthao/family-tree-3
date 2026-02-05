namespace backend.Application.Common.Models.MessageBus;

public class GraphGenerationStatusUpdateEvent
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // e.g., "Completed", "Failed"
    public string? OutputFilePath { get; set; }
    public string? ErrorMessage { get; set; }
}
