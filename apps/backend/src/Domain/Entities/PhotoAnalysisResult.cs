using System.Text.Json;
using backend.Domain.Common;
// Removed using backend.Domain.Common.Interfaces; assuming IAggregateRoot is in Common

namespace backend.Domain.Entities;

public class PhotoAnalysisResult : BaseAuditableEntity, IAggregateRoot
{
    // Id is inherited from BaseAuditableEntity
    public string OriginalUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Scene { get; set; } = string.Empty; // indoor/outdoor/...
    public string Event { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public JsonDocument? Faces { get; set; } // array of face objects
    public JsonDocument? Objects { get; set; }
    public string YearEstimate { get; set; } = string.Empty;
    // CreatedAt is inherited from BaseAuditableEntity

    // Navigation properties
    public ICollection<Memory> Memories { get; set; } = new List<Memory>();
}
