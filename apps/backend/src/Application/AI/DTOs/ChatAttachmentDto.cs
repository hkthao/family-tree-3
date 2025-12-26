namespace backend.Application.AI.DTOs;

/// <summary>
/// DTO chứa thông tin về tệp đính kèm trong tin nhắn trò chuyện.
/// </summary>
public class ChatAttachmentDto
{
    /// <summary>
    /// URL hoặc đường dẫn của tệp đính kèm.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Loại nội dung (MIME type) của tệp đính kèm (ví dụ: "image/jpeg", "application/pdf").
    /// </summary>
    public string ContentType { get; set; } = null!;
}
