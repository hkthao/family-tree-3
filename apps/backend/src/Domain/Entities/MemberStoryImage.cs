namespace backend.Domain.Entities;

public class MemberStoryImage : BaseAuditableEntity
{
    public Guid MemberStoryId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation property
    public MemberStory MemberStory { get; set; } = default!;
}
