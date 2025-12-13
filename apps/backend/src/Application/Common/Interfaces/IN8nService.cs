using backend.Application.AI.DTOs.Embeddings; // NEW USING for Embeddings DTOs
using backend.Application.AI.Models; // NEW USING
using backend.Application.Common.Models;
using backend.Application.Files.DTOs; // UPDATED DTOs PATH

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
    /// Gọi webhook tải ảnh lên của n8n để tải ảnh lên dịch vụ lưu trữ ảnh bên ngoài.
    /// </summary>
    /// <param name="dto">Đối tượng chứa dữ liệu ảnh và các tham số tải lên.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chứa phản hồi tải ảnh lên.</returns>
    Task<Result<ImageUploadResponseDto>> CallImageUploadWebhookAsync(ImageUploadWebhookDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Gọi webhook xử lý vector khuôn mặt của n8n để upsert.
    /// </summary>
    Task<Result<FaceVectorOperationResultDto>> CallUpsertFaceVectorWebhookAsync(UpsertFaceVectorOperationDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Gọi webhook xử lý vector khuôn mặt của n8n để search.
    /// </summary>
    Task<Result<FaceVectorOperationResultDto>> CallSearchFaceVectorWebhookAsync(SearchFaceVectorOperationDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Gọi webhook xử lý vector khuôn mặt của n8n để delete.
    /// </summary>
    Task<Result<FaceVectorOperationResultDto>> CallDeleteFaceVectorWebhookAsync(DeleteFaceVectorOperationDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Gọi webhook embeddings của n8n để tạo hoặc cập nhật embeddings từ dữ liệu family (thành viên, sự kiện, câu chuyện, tổng quan gia đình).
    /// </summary>
    /// <param name="dto">Đối tượng chứa dữ liệu để tạo embeddings.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả của lời gọi webhook.</returns>
    Task<Result<string>> CallEmbeddingsWebhookAsync(BaseEmbeddingsDto dto, CancellationToken cancellationToken);
}
