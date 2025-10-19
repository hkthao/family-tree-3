using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace backend.Application.Common.Services;

public class ConfigProvider : IConfigProvider
{
    private readonly IConfiguration _configuration;
    private readonly ISystemConfigurationService _systemConfigurationService;

    public ConfigProvider(IConfiguration configuration, ISystemConfigurationService systemConfigurationService)
    {
        _configuration = configuration;
        _systemConfigurationService = systemConfigurationService;
    }

    public T GetSection<T>() where T : new()
    {
        var sectionName = typeof(T).Name;

        // Try to get from database first
        var dbConfigResult = _systemConfigurationService.GetConfigurationAsync(sectionName).GetAwaiter().GetResult();
        if (dbConfigResult.IsSuccess)
        {
            try
            {
                // Deserialize the JSON string value from the database into the T object
                return JsonSerializer.Deserialize<T>(dbConfigResult.Value!.Value) ?? new T();
            }
            catch (JsonException)
            {
                // Log error and fall back to file/env config
                // _logger.LogError(ex, "Failed to deserialize config section {SectionName} from database.", sectionName);
            }
        }

        // Fallback to file/environment configuration
        return _configuration.GetSection(sectionName).Get<T>() ?? new T();
    }
}