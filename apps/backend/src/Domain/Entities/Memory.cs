using backend.Domain.Common;
// Removed using backend.Domain.Common.Interfaces; assuming IAggregateRoot is in Common
// Removed using backend.Domain.ValueObjects; as it's not used here

namespace backend.Domain.Entities;

public class Memory : BaseAuditableEntity, IAggregateRoot, ISoftDelete
{
    // Id is inherited from BaseAuditableEntity
    public Guid MemberId { get; set; }
    public string Title { get; set; } = string.Empty; // max 120
    public string Story { get; set; } = string.Empty; // long text
    public Guid? PhotoAnalysisId { get; set; }
    public string? PhotoUrl { get; set; } // optional (restored or original)
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string[] Keywords { get; set; } = Array.Empty<string>();
    // CreatedAt is inherited from BaseAuditableEntity

    // Navigation properties
    public Member Member { get; set; } = default!;
    public PhotoAnalysisResult? PhotoAnalysisResult { get; set; }
}
