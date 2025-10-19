using MediatR;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace backend.Application.SystemConfigurations.Commands.InitializeSystemConfigurations;

public class InitializeSystemConfigurationsCommandHandler : IRequestHandler<InitializeSystemConfigurationsCommand, Result>
{
    private readonly ILogger<InitializeSystemConfigurationsCommandHandler> _logger;
    private readonly ISystemConfigurationService _systemConfigurationService;
    private readonly IConfiguration _configuration;

    public InitializeSystemConfigurationsCommandHandler(
        ILogger<InitializeSystemConfigurationsCommandHandler> logger,
        ISystemConfigurationService systemConfigurationService,
        IConfiguration configuration)
    {
        _logger = logger;
        _systemConfigurationService = systemConfigurationService;
        _configuration = configuration;
    }

    public async Task<Result> Handle(InitializeSystemConfigurationsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to initialize system configurations from IConfiguration.");

        // Check if configurations already exist in the database
        var existingConfigs = await _systemConfigurationService.GetAllConfigurationsAsync();
        if (existingConfigs.IsSuccess && existingConfigs.Value != null && existingConfigs.Value.Any())
        {
            _logger.LogInformation("System configurations already exist in the database. Skipping initialization from IConfiguration.");
            return Result.Success();
        }

        var configurationsToSeed = new Dictionary<string, (string value, string valueType, string description)>();

        // Define sensitive keys that should not be stored in the database
        // var sensitiveKeys = new List<string>
        // {
        //     "APIKey", "Secret", "Password", "ConnectionStrings", "JwtSettings"
        // };

        foreach (var configEntry in _configuration.AsEnumerable())
        {
            var key = configEntry.Key;
            var value = configEntry.Value;

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            // Skip sensitive keys
            // if (sensitiveKeys.Any(sk => key.Contains(sk, StringComparison.OrdinalIgnoreCase)))
            // {
            //     _logger.LogDebug("Skipping sensitive configuration key: {Key}", key);
            //     continue;
            // }

            // Determine value type (simple heuristic for now)
            var valueType = "string";
            if (int.TryParse(value, out _)) valueType = "int";
            else if (bool.TryParse(value, out _)) valueType = "bool";
            else if (double.TryParse(value, out _)) valueType = "double";

            if (value.StartsWith("${") && value.EndsWith("}"))
                value = string.Empty;

            configurationsToSeed[key] = (value, valueType, $"Configuration loaded from IConfiguration.");
        }

        if (configurationsToSeed.Any())
        {
            var setResult = await _systemConfigurationService.SetConfigurationsAsync(configurationsToSeed);
            if (setResult.IsSuccess)
            {
                _logger.LogInformation("Successfully initialized {Count} system configurations from IConfiguration.", configurationsToSeed.Count);
                return Result.Success();
            }
            else
            {
                _logger.LogError("Failed to save system configurations from IConfiguration: {Error}", setResult.Error);
                return Result.Failure($"Failed to save system configurations from IConfiguration: {setResult.Error}");
            }
        }

        _logger.LogInformation("No configurations found in IConfiguration to initialize.");
        return Result.Success();
    }
}
