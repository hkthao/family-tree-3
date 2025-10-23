using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà cung cấp dịch vụ trò chuyện AI.
/// </summary>
public interface IChatProvider
{
    /// <summary>
    /// Tạo phản hồi từ mô hình trò chuyện AI dựa trên danh sách tin nhắn đã cho.
    /// </summary>
    /// <param name="messages">Danh sách các tin nhắn trò chuyện.</param>
    /// <returns>Phản hồi dạng chuỗi từ mô hình AI.</returns>
    Task<string> GenerateResponseAsync(List<ChatMessage> messages);
}
