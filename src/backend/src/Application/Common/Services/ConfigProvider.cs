using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace backend.Application.Common.Services;

/// <summary>
/// Cung cấp các phương thức để truy cập cấu hình ứng dụng.
/// </summary>
public class ConfigProvider(IConfiguration configuration) : IConfigProvider
{
    /// <summary>
    /// Đối tượng cấu hình ứng dụng.
    /// </summary>
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Lấy một phần cấu hình và ánh xạ nó sang một đối tượng kiểu T.
    /// </summary>
    /// <typeparam name="T">Kiểu của đối tượng cấu hình.</typeparam>
    /// <returns>Đối tượng cấu hình đã được ánh xạ, hoặc một đối tượng T mới nếu không tìm thấy cấu hình.</returns>
    public T GetSection<T>() where T : new()
    {
        var sectionName = typeof(T).Name;
        return _configuration.GetSection(sectionName).Get<T>() ?? new T();
    }
}
