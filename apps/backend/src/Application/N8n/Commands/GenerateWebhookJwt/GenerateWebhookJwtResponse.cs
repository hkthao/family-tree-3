namespace backend.Application.N8n.Commands.GenerateWebhookJwt;

public record GenerateWebhookJwtResponse
{
    /// <summary>
    /// Chuỗi JWT đã tạo.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Thời gian hết hạn của JWT.
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}
