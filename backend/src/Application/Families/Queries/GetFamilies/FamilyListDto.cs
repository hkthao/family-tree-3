using backend.Application.Common.Dtos;

namespace backend.Application.Families.Queries.GetFamilies;

public class FamilyListDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int TotalMembers { get; set; }
}