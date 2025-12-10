using backend.Domain.Common;

namespace backend.Domain.Entities;

public class MemberStoryImage : BaseAuditableEntity
{
    public Guid MemberStoryId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ResizedImageUrl { get; set; }
    public string? Caption { get; set; }

    // Navigation property
    public MemberStory MemberStory { get; set; } = default!;
}
