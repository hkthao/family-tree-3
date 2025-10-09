using backend.Application.Common.Dtos;

namespace backend.Application.Families.Queries.GetFamilyById;

public class FamilyDetailDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string Visibility { get; set; } = null!;
    public int TotalMembers { get; set; }
}
