namespace backend.Application.Common.Interfaces;

public interface IConfigurationProvider
{
    Task<T?> GetValue<T>(string key, T defaultValue);
    Task<T?> GetValue<T>(string key);
}
