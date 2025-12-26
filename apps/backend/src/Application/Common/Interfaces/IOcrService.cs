using backend.Application.Common.Models;
using backend.Application.Files.DTOs; // For OcrResultDto

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Giao diện cho dịch vụ OCR bên ngoài.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Thực hiện nhận dạng ký tự quang học (OCR) trên một tệp đã cho.
    /// </summary>
    /// <param name="fileBytes">Nội dung tệp dưới dạng mảng byte.</param>
    /// <param name="contentType">Loại nội dung (ví dụ: image/jpeg, application/pdf).</param>
    /// <param name="fileName">Tên tệp (ví dụ: document.pdf).</param>
    /// <param name="lang">Ngôn ngữ để thực hiện OCR (ví dụ: "eng", "vie", "eng+vie").</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chứa văn bản được OCR nếu thành công.</returns>
    Task<Result<OcrResultDto>> PerformOcrAsync(
        byte[] fileBytes,
        string contentType,
        string fileName,
        string lang = "eng+vie",
        CancellationToken cancellationToken = default);
}
