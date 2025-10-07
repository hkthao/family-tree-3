using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between a Family and a UserProfile,
/// including the user's role within that family.
/// </summary>
public class FamilyUser : BaseEntity
{
    public Guid FamilyId { get; set; } // Changed from string to Guid
    public Family Family { get; set; } = null!;

    public Guid UserProfileId { get; set; } // Changed from string to Guid
    public UserProfile UserProfile { get; set; } = null!;

    public FamilyRole Role { get; set; }
}