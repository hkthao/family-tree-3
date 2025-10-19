namespace backend.Application.SystemConfigurations.Queries;

public class SystemConfigurationDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? ValueType { get; set; }
    public string? Description { get; set; }
}