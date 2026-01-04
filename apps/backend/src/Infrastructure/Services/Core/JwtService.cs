using backend.Application.Common.Interfaces;
using backend.Infrastructure.Auth;

namespace backend.Infrastructure.Services;

/// <summary>
/// Triển khai của IJwtService sử dụng IJwtHelperFactory để tạo JWT.
/// </summary>
public class JwtService : IJwtService
{
    private readonly IJwtHelperFactory _jwtHelperFactory;

    public JwtService(IJwtHelperFactory jwtHelperFactory)
    {
        _jwtHelperFactory = jwtHelperFactory;
    }

    /// <inheritdoc />
    public string GenerateToken(string subject, DateTime expires, string secret)
    {
        var jwtHelper = _jwtHelperFactory.Create(secret);
        return jwtHelper.GenerateToken(subject, expires);
    }
}
