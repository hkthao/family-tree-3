using FamilyTree.Application.Common.Interfaces;
using FamilyTree.Application.SystemConfigurations.Queries.GetSystemConfiguration;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FamilyTree.Infrastructure.Services;

public class ConfigurationProvider : IConfigurationProvider
{
    private readonly IMediator _mediator;
    private readonly AppSettings _appSettings;
    private readonly IMemoryCache _cache;

    private const string CacheKeyPrefix = "SystemConfig_";

    public ConfigurationProvider(IMediator mediator, IOptions<AppSettings> appSettings, IMemoryCache cache)
    {
        _mediator = mediator;
        _appSettings = appSettings.Value;
        _cache = cache;
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        return GetValueInternal<T>(key, defaultValue);
    }

    public T GetValue<T>(string key)
    {
        return GetValueInternal<T>(key, default(T));
    }

    private T GetValueInternal<T>(string key, T defaultValue)
    {
        // Priority 1: SystemConfig (from database, with caching)
        var systemConfigValue = GetSystemConfigValue<T>(key);
        if (systemConfigValue != null)
        {
            return systemConfigValue;
        }

        // Priority 2: AppSettings (from appsettings.json)
        var appSettingsValue = GetAppSettingsValue<T>(key);
        if (appSettingsValue != null)
        {
            return appSettingsValue;
        }

        return defaultValue;
    }

    private T? GetSystemConfigValue<T>(string key)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";
        if (_cache.TryGetValue(cacheKey, out T? cachedValue))
        {
            return cachedValue;
        }

        var result = _mediator.Send(new GetSystemConfigurationQuery(key)).Result; // .Result to unwrap Task synchronously

        if (result.Succeeded && result.Data != null)
        {
            var value = ConvertValue<T>(result.Data.Value, result.Data.ValueType);
            _cache.Set(cacheKey, value, TimeSpan.FromMinutes(5)); // Cache for 5 minutes
            return value;
        }

        return default;
    }

    private T? GetAppSettingsValue<T>(string key)
    {
        // Use reflection to get value from AppSettings based on key
        // This assumes AppSettings has properties matching the keys
        var property = typeof(AppSettings).GetProperty(key);
        if (property != null && property.CanRead)
        {
            var value = property.GetValue(_appSettings);
            if (value != null)
            {
                return ConvertValue<T>(value.ToString()!, "string"); // AppSettings values are typically strings
            }
        }
        return default;
    }

    private T? ConvertValue<T>(string value, string valueType)
    {
        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }
            if (typeof(T) == typeof(int) && valueType.Equals("int", StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)int.Parse(value);
            }
            if (typeof(T) == typeof(bool) && valueType.Equals("bool", StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)bool.Parse(value);
            }
            if (valueType.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Deserialize<T>(value);
            }
        }
        catch (Exception ex)
        {
            // Log error: Could not convert value
            Console.WriteLine($"Error converting value '{value}' to type {typeof(T).Name}: {ex.Message}");
        }
        return default;
    }
}
