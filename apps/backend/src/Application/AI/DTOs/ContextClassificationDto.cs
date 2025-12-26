using backend.Application.AI.Enums;

namespace backend.Application.AI.DTOs;

/// <summary>
/// DTO để biểu diễn kết quả phân loại ngữ cảnh từ AI.
/// </summary>
public class ContextClassificationDto
{
    /// <summary>
    /// Loại ngữ cảnh được xác định bởi AI.
    /// </summary>
    public ContextType Context { get; set; }

    /// <summary>
    /// Lý do (tùy chọn) mà AI đưa ra để phân loại ngữ cảnh.
    /// </summary>
    public string? Reasoning { get; set; }
}
