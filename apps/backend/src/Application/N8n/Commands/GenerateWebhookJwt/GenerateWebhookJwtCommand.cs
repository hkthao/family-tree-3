using backend.Application.Common.Models;

namespace backend.Application.N8n.Commands.GenerateWebhookJwt;

public record GenerateWebhookJwtCommand : IRequest<Result<GenerateWebhookJwtResponse>>
{
    /// <summary>
    /// Chủ đề (subject) của JWT, thường là ID của người dùng hoặc ID webhook.
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Thời gian hết hạn của JWT tính bằng phút. Mặc định là 60 phút.
    /// </summary>
    public int ExpiresInMinutes { get; init; } = 60;
}
