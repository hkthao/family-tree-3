using backend.Domain.Enums;
using backend.Domain.Entities;

namespace backend.Application.Relations;

public class RelationDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public RelationType Type { get; set; }
    public string Description { get; set; } = null!;
    public RelationLineage Lineage { get; set; }
    public bool SpecialRelation { get; set; }
    public NamesByRegionDto NamesByRegion { get; set; } = null!;
}

public class NamesByRegionDto
{
    public string North { get; set; } = null!;
    public object Central { get; set; } = null!; // Can be string or array of strings
    public object South { get; set; } = null!; // Can be string or array of strings
}
