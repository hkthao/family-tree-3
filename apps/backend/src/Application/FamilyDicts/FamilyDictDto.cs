using backend.Domain.Enums;

namespace backend.Application.FamilyDicts;

public class FamilyDictDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public FamilyDictType Type { get; set; }
    public string Description { get; set; } = null!;
    public FamilyDictLineage Lineage { get; set; }
    public bool SpecialRelation { get; set; }
    public NamesByRegionDto NamesByRegion { get; set; } = null!;
}

public class NamesByRegionDto
{
    public string North { get; set; } = null!;
    public object Central { get; set; } = null!; // Can be string or array of strings
    public object South { get; set; } = null!; // Can be string or array of strings
}
