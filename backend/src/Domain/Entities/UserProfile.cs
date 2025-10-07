namespace backend.Domain.Entities;

/// <summary>
/// Represents a user profile mapped from an external authentication provider (e.g., Auth0).
/// </summary>
public class UserProfile : BaseAuditableEntity
{
    /// <summary>
    /// The unique identifier for the user from the external authentication provider.
    /// </summary>
    public string Auth0UserId { get; set; } = null!;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// The user's display name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Navigation property for families the user is associated with.
    /// </summary>
    public ICollection<FamilyUser> FamilyUsers { get; set; } = new HashSet<FamilyUser>();
}
