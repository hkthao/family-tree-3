using backend.Application.Common.Models;

namespace backend.Application.Services;

public interface IConfigurationProvider
{
    Task<Result<string>> GetConfigurationValueAsync(string key, CancellationToken cancellationToken = default);
    Task<Result<T>> GetConfigurationValueAsync<T>(string key, CancellationToken cancellationToken = default);
}
