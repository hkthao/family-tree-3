using backend.Application.AI.DTOs; // UPDATED USING
using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho dịch vụ tương tác với n8n.
/// </summary>
public interface IN8nService
{
    /// <summary>
    /// Gọi webhook chat của n8n để gửi tin nhắn và nhận phản hồi từ AI Assistant.
    /// </summary>
    /// <param name="sessionId">ID phiên trò chuyện.</param>
    /// <param name="message">Tin nhắn người dùng.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chứa phản hồi từ AI Assistant.</returns>
    Task<Result<string>> CallChatWebhookAsync(string sessionId, string message, CancellationToken cancellationToken);

    /// <summary>
    /// Gọi webhook embedding của n8n để xử lý dữ liệu và tạo embedding.
    /// </summary>
    /// <param name="dto">Đối tượng chứa dữ liệu embedding.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chứa ID thành viên hoặc thông báo lỗi.</returns>
    Task<Result<string>> CallEmbeddingWebhookAsync(EmbeddingWebhookDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Gọi webhook tải ảnh lên của n8n để tải ảnh lên dịch vụ lưu trữ ảnh bên ngoài.
    /// </summary>
    /// <param name="dto">Đối tượng chứa dữ liệu ảnh và các tham số tải lên.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chứa danh sách phản hồi tải ảnh lên.</returns>
    Task<Result<List<ImageUploadResponseDto>>> CallImageUploadWebhookAsync(ImageUploadWebhookDto dto, CancellationToken cancellationToken);
}
