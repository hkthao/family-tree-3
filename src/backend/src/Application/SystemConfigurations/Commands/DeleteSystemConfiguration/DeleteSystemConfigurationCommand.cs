using backend.Application.Common.Models;

namespace backend.Application.SystemConfigurations.Commands.DeleteSystemConfiguration;

public record DeleteSystemConfigurationCommand(Guid Id) : IRequest<Result>;
