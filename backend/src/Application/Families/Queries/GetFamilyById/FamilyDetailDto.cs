using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.Families.Queries.GetFamilyById;

public class FamilyDetailDto : IMapFrom<Family>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string Visibility { get; set; } = null!;
    public int TotalMembers { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}