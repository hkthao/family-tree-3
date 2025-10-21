using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface ISystemConfigurationService
{
    Task<Result<SystemConfiguration>> GetConfigurationAsync(string key);
    Task<Result<List<SystemConfiguration>>> GetAllConfigurationsAsync();
    Task<Result> SetConfigurationAsync(string key, string value, string valueType, string description = "");
    Task<Result> SetConfigurationsAsync(Dictionary<string, (string value, string valueType, string description)> configurations);
    Task<Result> DeleteConfigurationAsync(string key);
    Task<bool> ConfigurationExistsAsync(string key);
}
