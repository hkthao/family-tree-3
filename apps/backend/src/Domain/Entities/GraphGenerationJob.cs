namespace backend.Domain.Entities;

public class GraphGenerationJob : BaseAuditableEntity
{
    public string JobId { get; set; } = Guid.NewGuid().ToString("N"); // Unique ID for the job
    public Guid FamilyId { get; set; }
    public Guid RootMemberId { get; set; }
    public string DotFilePath { get; set; } = string.Empty;
    public string OutputFilePath { get; set; } = string.Empty; // Path to the generated PDF/image
    public string Status { get; set; } = "Pending"; // e.g., Pending, Processing, Completed, Failed
    public string ErrorMessage { get; set; } = string.Empty; // To store error messages if any
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedProcessingAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
