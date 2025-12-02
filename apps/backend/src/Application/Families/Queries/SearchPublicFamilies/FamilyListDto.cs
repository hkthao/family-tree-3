using backend.Application.Common.Dtos;

namespace backend.Application.Families.Queries.SearchPublicFamilies;

public class FamilyListDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; } // Added
    public int TotalMembers { get; set; }
    public int TotalGenerations { get; set; } // Added
    public string Visibility { get; set; } = null!; // Added
}