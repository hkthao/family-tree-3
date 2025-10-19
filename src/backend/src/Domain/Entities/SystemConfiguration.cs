namespace backend.Domain.Entities;

public class SystemConfiguration : BaseAuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}