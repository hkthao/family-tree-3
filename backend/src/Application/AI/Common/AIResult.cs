using backend.Domain.Enums;

namespace backend.Application.AI.Common;

/// <summary>
/// Represents the result from an AI content generation service.
/// </summary>
public class AIResult
{
    public string Content { get; set; } = null!;
    public int TokensUsed { get; set; }
    public AIProviderType Provider { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
}
