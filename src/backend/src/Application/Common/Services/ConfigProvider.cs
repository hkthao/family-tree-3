using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace backend.Application.Common.Services;

public class ConfigProvider(IConfiguration configuration) : IConfigProvider
{
    private readonly IConfiguration _configuration = configuration;

    public T GetSection<T>() where T : new()
    {
        var sectionName = typeof(T).Name;
        return _configuration.GetSection(sectionName).Get<T>() ?? new T();
    }
}
