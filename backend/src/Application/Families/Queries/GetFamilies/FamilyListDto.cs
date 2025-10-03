using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Application.Common.Dtos;

namespace backend.Application.Families.Queries.GetFamilies;

public class FamilyListDto : BaseAuditableDto, IMapFrom<Family>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int TotalMembers { get; set; }
}