using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.Families;

public class FamilyDto : IMapFrom<Family> // IMapFrom will create a map from Family to FamilyDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
}
