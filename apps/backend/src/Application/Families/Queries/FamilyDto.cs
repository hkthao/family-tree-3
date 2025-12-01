using backend.Application.Common.Dtos;

namespace backend.Application.Families.Queries;

public class FamilyDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public int TotalMembers { get; set; } = 0;
    public int? TotalGenerations { get; set; } = null;
    public string? Visibility { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string> ValidationErrors { get; set; } = new List<string>();
}
