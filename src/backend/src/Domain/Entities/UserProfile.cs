namespace backend.Domain.Entities;

/// <summary>
/// Represents a user profile mapped from an external authentication provider (e.g., Auth0).
/// </summary>
public class UserProfile : BaseAuditableEntity
{
    /// <summary>
    /// Foreign key to the User entity.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Navigation property to the User entity.
    /// </summary>
    public User User { get; private set; } = null!;

    // Private constructor for EF Core
    private UserProfile() { }

    public UserProfile(Guid userId)
    {
        UserId = userId;
    }
    /// <summary>
    /// The unique identifier for the user from the external authentication provider.
    /// </summary>
    public string ExternalId { get; private set; } = null!;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; private set; } = null!;

    /// <summary>
    /// The user's display name.
    /// </summary>
    public string Name { get; private set; } = null!;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Phone { get; private set; }

    /// <summary>
    /// The URL of the user's avatar.
    /// </summary>
    public string? Avatar { get; private set; }

    public void Update(string externalId, string email, string name, string firstName, string lastName, string phone, string avatar)
    {
        ExternalId = externalId;
        Email = email;
        Name = name;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Avatar = avatar;
    }
}
