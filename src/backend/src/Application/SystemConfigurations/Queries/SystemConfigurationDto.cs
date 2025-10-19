namespace backend.Application.SystemConfigurations.Queries.SystemConfigurationDto;

public class SystemConfigurationDto
{
    public Guid Id { get; init; }
    public string Key { get; init; } = null!;
    public string Value { get; init; } = null!;
    public string? Description { get; init; }
    public string ValueType { get; init; } = "string";
}
