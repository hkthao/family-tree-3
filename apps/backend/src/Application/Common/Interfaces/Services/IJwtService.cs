namespace backend.Application.Common.Interfaces.Services;

/// <summary>
/// Dịch vụ để tạo và quản lý JWT.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Tạo một JWT token.
    /// </summary>
    /// <param name="subject">Chủ đề của token (ví dụ: sessionId).</param>
    /// <param name="expires">Thời gian hết hạn của token.</param>
    /// <param name="secret">Chuỗi bí mật JWT.</param>
    /// <returns>Chuỗi JWT token đã tạo.</returns>
    string GenerateToken(string subject, DateTime expires, string secret);
}
