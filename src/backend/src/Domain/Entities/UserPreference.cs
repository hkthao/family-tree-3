using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class UserPreference : BaseAuditableEntity
{
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public Theme Theme { get; set; } = Theme.Light;
    public Language Language { get; set; } = Language.English;
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool SmsNotificationsEnabled { get; set; } = false;
    public bool InAppNotificationsEnabled { get; set; } = true;
}
