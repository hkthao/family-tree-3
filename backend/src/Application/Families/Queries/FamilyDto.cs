using backend.Domain.Entities;

namespace backend.Application.Families.Queries;

public class FamilyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string Visibility { get; set; } = null!;
    public int TotalMembers { get; set; }

    // Manual mapping from Family entity to FamilyDto
    public static FamilyDto FromEntity(Family family)
    {
        return new FamilyDto
        {
            Id = family.Id,
            Name = family.Name,
            Description = family.Description,
            AvatarUrl = family.AvatarUrl,
            Visibility = family.Visibility,
            TotalMembers = family.TotalMembers
        };
    }
}