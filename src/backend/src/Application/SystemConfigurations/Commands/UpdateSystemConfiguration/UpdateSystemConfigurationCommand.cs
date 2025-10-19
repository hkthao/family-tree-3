using backend.Application.Common.Models;

namespace backend.Application.SystemConfigurations.Commands.UpdateSystemConfiguration;

public record UpdateSystemConfigurationCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
