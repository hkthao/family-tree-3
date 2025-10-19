using backend.Application.Common.Models;

namespace backend.Application.SystemConfigurations.Commands.CreateSystemConfiguration;

public record CreateSystemConfigurationCommand : IRequest<Result<Guid>>
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
