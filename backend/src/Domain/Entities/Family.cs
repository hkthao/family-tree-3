namespace FamilyTree.Domain.Entities;

/// <summary>
/// Represents a family or clan.
/// </summary>
public class Family : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
    public string Visibility { get; set; } = "Private"; // e.g., Private, Public
}