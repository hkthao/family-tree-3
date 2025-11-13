using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between a Family and a UserProfile,
/// including the user's role within that family.
/// </summary>
public class FamilyUser : BaseEntity
{
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public FamilyRole Role { get; set; }

    // Private constructor for EF Core
    private FamilyUser() { }

    public FamilyUser(Guid familyId, Guid userId, FamilyRole role)
    {
        FamilyId = familyId;
        UserId = userId;
        Role = role;
    }
}
