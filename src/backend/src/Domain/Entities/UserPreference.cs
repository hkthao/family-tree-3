using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class UserPreference : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Theme Theme { get; private set; } = Theme.Dark;
    public Language Language { get; private set; } = Language.Vietnamese;

    // Private constructor for EF Core
    private UserPreference() { }

    public UserPreference(Guid userId)
    {
        UserId = userId;
    }

    public void Update(Theme theme, Language language)
    {
        Theme = theme;
        Language = language;
    }
}
