using backend.Application.Common.Models;

namespace backend.Application.AI.Chat.Queries;

/// <summary>
/// Đại diện cho một truy vấn trò chuyện với trợ lý AI.
/// </summary>
/// <param name="UserMessage">Tin nhắn của người dùng gửi đến trợ lý AI.</param>
/// <param name="SessionId">ID phiên trò chuyện hiện tại (tùy chọn).</param>
public record ChatWithAssistantQuery(string UserMessage, string? SessionId = null) : IRequest<Result<ChatResponse>>;
