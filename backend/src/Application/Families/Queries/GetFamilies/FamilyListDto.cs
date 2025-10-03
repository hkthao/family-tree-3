using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.Families.Queries.GetFamilies;

public class FamilyListDto : IMapFrom<Family>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int TotalMembers { get; set; }
    public DateTime Created { get; set; }
}