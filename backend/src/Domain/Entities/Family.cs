namespace backend.Domain.Entities;

/// <summary>
/// Represents a family or clan.
/// </summary>
public class Family : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    /// <summary>
    /// Dia chi
    /// </summary>
    public string? Address { get; set; }

    public string? AvatarUrl { get; set; }
    public string Visibility { get; set; } = "Private"; // e.g., Private, Public
    public int TotalMembers { get; set; }

    /// <summary>
    /// Navigation property for users associated with this family.
    /// </summary>
    public ICollection<FamilyUser> FamilyUsers { get; set; } = new HashSet<FamilyUser>();
}
