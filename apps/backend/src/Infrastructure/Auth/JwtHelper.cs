using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace backend.Infrastructure.Auth;

/// <summary>
/// Helper để tạo và xác thực JWT.
/// </summary>
public class JwtHelper
{
    private readonly string _jwtSecret;

    public JwtHelper(string secret)
    {
        _jwtSecret = secret;
    }

    /// <summary>
    /// Tạo một JWT token.
    /// </summary>
    /// <param name="subject">Chủ đề của token (ví dụ: sessionId).</param>
    /// <param name="expires">Thời gian hết hạn của token.</param>
    /// <returns>Chuỗi JWT token đã tạo.</returns>
    public string GenerateToken(string subject, DateTime expires)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, subject) }),
            Expires = expires,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
