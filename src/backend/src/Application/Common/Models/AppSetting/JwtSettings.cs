namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho JSON Web Token (JWT).
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Cơ quan phát hành (Authority) JWT.
    /// </summary>
    public string Authority { get; set; } = null!;
    /// <summary>
    /// Đối tượng người dùng (Audience) của JWT.
    /// </summary>
    public string Audience { get; set; } = null!;
    /// <summary>
    /// Không gian tên (Namespace) được sử dụng trong JWT.
    /// </summary>
    public string Namespace { get; set; } = null!;
}
