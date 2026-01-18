using backend.Application.Common.Models;
using backend.Application.Files.DTOs;

namespace backend.Application.OCR.Commands;

/// <summary>
/// Lệnh để xử lý một tệp bằng dịch vụ OCR.
/// </summary>
public record ProcessOcrFileCommand : IRequest<Result<OcrResultDto>>
{
    /// <summary>
    /// Nội dung tệp dưới dạng mảng byte.
    /// </summary>
    public byte[]? FileBytes { get; init; } // Made nullable

    /// <summary>
    /// URL của tệp cần xử lý.
    /// </summary>
    public string? FileUrl { get; init; } // New property

    /// <summary>
    /// Loại nội dung của tệp (ví dụ: image/jpeg, application/pdf).
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// Tên tệp gốc.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Ngôn ngữ để thực hiện OCR (mặc định: "eng+vie").
    /// </summary>
    public string Lang { get; init; } = "eng+vie";
}
