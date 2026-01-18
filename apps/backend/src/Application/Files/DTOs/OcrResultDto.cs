namespace backend.Application.Files.DTOs;

/// <summary>
/// DTO để biểu diễn kết quả từ dịch vụ OCR.
/// </summary>
public class OcrResultDto
{
    /// <summary>
    /// Cho biết liệu hoạt động OCR có thành công hay không.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Văn bản được trích xuất từ tài liệu đã OCR.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Mảng byte của tệp đã được xử lý (ví dụ: hình ảnh đã tải xuống, PDF).
    /// </summary>
    public byte[]? ProcessedBytes { get; set; }
}
