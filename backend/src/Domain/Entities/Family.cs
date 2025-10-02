using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Represents a family or clan.
/// </summary>
public class Family : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public string? AvatarUrl { get; set; }
    public FamilyVisibility Visibility { get; set; } = FamilyVisibility.Private;
    public int TotalMembers { get; set; }
}
