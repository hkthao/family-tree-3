using backend.Application.AI.DTOs;
using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.AI.Commands.DetermineChatContext;

/// <summary>
/// Lệnh để xác định ngữ cảnh của một tin nhắn chat từ người dùng.
/// </summary>
public record DetermineChatContextCommand : IRequest<Result<ContextClassificationDto>>
{
    /// <summary>
    /// Tin nhắn chat từ người dùng cần phân tích ngữ cảnh.
    /// </summary>
    public string ChatMessage { get; init; } = string.Empty;

    /// <summary>
    /// ID phiên chat, có thể dùng để tạo JWT hoặc theo dõi.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;
}
