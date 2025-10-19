using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace backend.Application.Common.Services;

public class ConfigProvider : IConfigProvider
{
    private readonly IConfiguration _configuration;
    private readonly ISystemConfigurationService _systemConfigurationService;
    private readonly ILogger<ConfigProvider> _logger;

    public ConfigProvider(IConfiguration configuration, ISystemConfigurationService systemConfigurationService, ILogger<ConfigProvider> logger)
    {
        _configuration = configuration;
        _systemConfigurationService = systemConfigurationService;
        _logger = logger;
    }

    public T GetSection<T>() where T : new()
    {
        var sectionName = typeof(T).Name;

        // Try to get from database first
        var dbConfigResult = _systemConfigurationService.GetConfigurationAsync(sectionName).GetAwaiter().GetResult();
        if (dbConfigResult.IsSuccess && dbConfigResult.Value!=null && !string.IsNullOrEmpty(dbConfigResult.Value.Value))
        {
            try
            {
                // Deserialize the JSON string value from the database into the T object
                return JsonSerializer.Deserialize<T>(dbConfigResult.Value.Value) ?? new T();
            }
            catch (JsonException ex)
            {
                // Log error and fall back to file/env config
                _logger.LogError(ex, "Failed to deserialize config section {SectionName} from database.", sectionName);
            }
        }

        // Fallback to file/environment configuration
        return _configuration.GetSection(sectionName).Get<T>() ?? new T();
    }
}