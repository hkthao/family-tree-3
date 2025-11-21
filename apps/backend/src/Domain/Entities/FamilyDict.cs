using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class FamilyDict : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public FamilyDictType Type { get; set; }
    public string Description { get; set; } = null!;
    public FamilyDictLineage Lineage { get; set; }
    public bool SpecialRelation { get; set; }
    public NamesByRegion NamesByRegion { get; set; } = null!; // This will be stored as JSON
}

public class NamesByRegion
{
    public string North { get; set; } = null!;
    public object Central { get; set; } = null!; // Can be string or array of strings
    public object South { get; set; } = null!; // Can be string or array of strings
}
