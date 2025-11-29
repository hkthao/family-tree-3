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
    public string? RawInput { get; set; } // NEW: Raw input text from user for AI generation
    public string? StoryStyle { get; set; } // NEW: Style for AI story generation (e.g., nostalgic, warm)
    public string? Perspective { get; set; } // NEW: Perspective for AI story generation (e.g., firstPerson, neutralPersonal)

    // Navigation properties
    public Member Member { get; set; } = default!;
}

