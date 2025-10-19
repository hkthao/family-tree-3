using FamilyTree.Domain.Common;

namespace FamilyTree.Domain.Entities;

public class SystemConfiguration : BaseAuditableEntity
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Description { get; set; }
    public string ValueType { get; set; } = "string"; // e.g., "string", "int", "bool", "json"
}
