namespace backend.Infrastructure.Auth;

/// <summary>
/// Interface cho factory tạo JwtHelper.
/// </summary>
public interface IJwtHelperFactory
{
    /// <summary>
    /// Tạo một thể hiện mới của JwtHelper với secret được cung cấp.
    /// </summary>
    /// <param name="secret">Chuỗi bí mật JWT.</param>
    /// <returns>Một thể hiện của JwtHelper.</returns>
    JwtHelper Create(string secret);
}
