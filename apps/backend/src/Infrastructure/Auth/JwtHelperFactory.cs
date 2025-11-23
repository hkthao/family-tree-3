using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Auth;

/// <summary>
/// Factory để tạo JwtHelper, sử dụng N8nSettings để cung cấp secret mặc định.
/// </summary>
public class JwtHelperFactory : IJwtHelperFactory
{
    private readonly N8nSettings _n8nSettings;

    public JwtHelperFactory(IOptions<N8nSettings> n8nSettings)
    {
        _n8nSettings = n8nSettings.Value;
    }

    /// <summary>
    /// Tạo một JwtHelper với secret được cung cấp.
    /// </summary>
    /// <param name="secret">Chuỗi bí mật JWT.</param>
    /// <returns>Một thể hiện của JwtHelper.</returns>
    public JwtHelper Create(string secret)
    {
        return new JwtHelper(secret);
    }
}
