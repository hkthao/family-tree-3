using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services;

public class SystemConfigurationService(IApplicationDbContext context, ILogger<SystemConfigurationService> logger) : ISystemConfigurationService
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<SystemConfigurationService> _logger = logger;

    public async Task<Result<SystemConfiguration>> GetConfigurationAsync(string key)
    {
        var config = await _context.SystemConfigurations.FirstOrDefaultAsync(sc => sc.Key == key);
        return config == null
            ? Result<SystemConfiguration>.Failure($"Configuration with key '{key}' not found.")
            : Result<SystemConfiguration>.Success(config);
    }

    public async Task<Result<List<SystemConfiguration>>> GetAllConfigurationsAsync()
    {
        var configs = await _context.SystemConfigurations.ToListAsync();
        return Result<List<SystemConfiguration>>.Success(configs);
    }

    public async Task<Result> SetConfigurationAsync(string key, string value, string valueType, string description = "")
    {
        var config = await _context.SystemConfigurations.FirstOrDefaultAsync(sc => sc.Key == key);
        if (config == null)
        {
            config = new SystemConfiguration
            {
                Key = key,
                Value = value,
                ValueType = valueType,
                Description = description,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };
            _context.SystemConfigurations.Add(config);
        }
        else
        {
            config.Value = value;
            config.ValueType = valueType;
            config.Description = description;
            config.LastModified = DateTime.UtcNow;
            config.LastModifiedBy = "System";
            _context.SystemConfigurations.Update(config);
        }

        try
        {
            await _context.SaveChangesAsync(CancellationToken.None);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting system configuration for key {Key}", key);
            return Result.Failure($"Error setting system configuration: {ex.Message}");
        }
    }

    public async Task<Result> SetConfigurationsAsync(Dictionary<string, (string value, string valueType, string description)> configurations)
    {
        foreach (var entry in configurations)
        {
            var key = entry.Key;
            var (value, valueType, description) = entry.Value;
            var result = await SetConfigurationAsync(key, value, valueType, description);
            if (!result.IsSuccess)
            {
                return result; // Return on first failure
            }
        }
        return Result.Success();
    }

    public async Task<Result> DeleteConfigurationAsync(string key)
    {
        var config = await _context.SystemConfigurations.FirstOrDefaultAsync(sc => sc.Key == key);
        if (config == null)
        {
            return Result.Failure($"Configuration with key '{key}' not found.");
        }

        _context.SystemConfigurations.Remove(config);
        try
        {
            await _context.SaveChangesAsync(CancellationToken.None);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting system configuration for key {Key}", key);
            return Result.Failure($"Error deleting system configuration: {ex.Message}");
        }
    }

    public async Task<bool> ConfigurationExistsAsync(string key)
    {
        return await _context.SystemConfigurations.AnyAsync(sc => sc.Key == key);
    }
}
