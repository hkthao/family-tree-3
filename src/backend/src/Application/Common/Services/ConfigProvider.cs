using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Services;

public class ConfigProvider(IConfiguration configuration, ISystemConfigurationService systemConfigurationService, ILogger<ConfigProvider> logger) : IConfigProvider
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ISystemConfigurationService _systemConfigurationService = systemConfigurationService;
    private readonly ILogger<ConfigProvider> _logger = logger;

    public T GetSection<T>() where T : new()
    {
        var sectionName = typeof(T).Name;

        // Try to get from database first
        var allDbConfigsResult = _systemConfigurationService.GetAllConfigurationsAsync().GetAwaiter().GetResult();

        if (allDbConfigsResult.IsSuccess && allDbConfigsResult.Value != null)
        {
            var sectionConfigs = allDbConfigsResult.Value
                .Where(c => c.Key.StartsWith(sectionName + ":")) // Filter configs belonging to this section
                .ToDictionary(c => c.Key[(sectionName.Length + 1)..], c => (string?)c.Value);

            if (sectionConfigs.Any())
            {
                try
                {
                    // Create a temporary IConfiguration from the dictionary
                    var configBuilder = new ConfigurationBuilder();
                    configBuilder.AddInMemoryCollection(sectionConfigs);
                    var tempConfig = configBuilder.Build();

                    // Bind the section to the T object
                    return tempConfig.Get<T>() ?? new T();
                }
                catch (Exception ex) // Catch general exception during configuration binding
                {
                    _logger.LogError(ex, "Failed to bind config section {SectionName} from database configurations.", sectionName);
                }
            }
        }

        // Fallback to file/environment configuration
        return _configuration.GetSection(sectionName).Get<T>() ?? new T();
    }
}
