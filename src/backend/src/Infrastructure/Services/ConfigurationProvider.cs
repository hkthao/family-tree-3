using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Application.SystemConfigurations.Queries.GetSystemConfiguration;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

public class ConfigurationProvider(IMediator mediator, IOptions<AppSettings> appSettings, IMemoryCache cache) : IConfigurationProvider
{
    private readonly IMediator _mediator = mediator;
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly IMemoryCache _cache = cache;

    private const string CacheKeyPrefix = "SystemConfig_";

    public async Task<T?> GetValue<T>(string key, T defaultValue)
    {
        return await GetValueInternal(key, defaultValue);
    }

    public async Task<T?> GetValue<T>(string key)
    {
        return await GetValueInternal(key, default(T));
    }

    private async Task<T?> GetValueInternal<T>(string key, T? defaultValue)
    {
        // Priority 1: SystemConfig (from database, with caching)
        var systemConfigValue = await GetSystemConfigValue<T>(key);
        if (systemConfigValue != null)
        {
            return systemConfigValue;
        }

        // Priority 2: AppSettings (from appsettings.json)
        var appSettingsValue = GetAppSettingsValue<T>(key); // This method is not async, so no await needed here
        return appSettingsValue != null ? appSettingsValue : defaultValue ?? default;
    }

    private async Task<T?> GetSystemConfigValue<T>(string key)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";
        if (_cache.TryGetValue(cacheKey, out T? cachedValue))
        {
            return cachedValue;
        }

        var result = await _mediator.Send(new GetSystemConfigurationQuery(key));

        if (result.IsSuccess && result.Value != null)
        {
            var value = ConvertValue<T>(result.Value.Value, result.Value.ValueType);
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

    private T? ConvertValue<T>(string? value, string? valueType)
    {
        if (value == null || valueType == null)
        {
            return default;
        }

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
