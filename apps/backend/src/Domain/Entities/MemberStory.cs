// Removed using backend.Domain.Common.Interfaces; assuming IAggregateRoot is in Common
// Removed using backend.Domain.ValueObjects; as it's not used here

namespace backend.Domain.Entities;

public class MemberStory : BaseAuditableEntity, ISoftDelete
{
    // Id is inherited from BaseAuditableEntity
    public Guid MemberId { get; set; }
    public string Title { get; set; } = string.Empty; // max 120
    public string Story { get; set; } = string.Empty; // long text
    public string? OriginalImageUrl { get; set; } // NEW
    public string? ResizedImageUrl { get; set; } // NEW

    // Navigation properties
    public Member Member { get; set; } = default!;
}

