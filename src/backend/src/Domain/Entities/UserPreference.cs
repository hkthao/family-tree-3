using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class UserPreference : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Theme Theme { get; private set; } = Theme.Light;
    public Language Language { get; private set; } = Language.English;

    // Private constructor for EF Core
    private UserPreference() { }

    public UserPreference(Guid userId)
    {
        UserId = userId;
    }

    public void Update(string theme, string language)
    {
        Theme = Enum.Parse<Theme>(theme);
        Language = Enum.Parse<Language>(language);
    }
}
