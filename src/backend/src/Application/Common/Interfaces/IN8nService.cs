using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho dịch vụ tương tác với n8n.
/// </summary>
public interface IN8nService
{
    /// <summary>
    /// Gọi webhook chat trên n8n với tin nhắn và lịch sử cuộc trò chuyện.
    /// </summary>
    /// <param name="message">Tin nhắn của người dùng.</param>
    /// <param name="history">Lịch sử cuộc trò chuyện.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Kết quả chứa câu trả lời từ AI.</returns>
    Task<Result<string>> CallChatWebhookAsync(string message, List<ChatMessage> history, CancellationToken cancellationToken);
}
