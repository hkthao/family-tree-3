namespace backend.Domain.Entities;

public class UserConfig : BaseAuditableEntity
{
    public Guid UserProfileId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation property
    public UserProfile UserProfile { get; set; } = null!;
}
