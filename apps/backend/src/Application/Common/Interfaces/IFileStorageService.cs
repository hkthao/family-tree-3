using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Lưu file vào bộ nhớ.
    /// </summary>
    /// <param name="fileStream">Stream của file cần lưu.</param>
    /// <param name="fileName">Tên file.</param>
    /// <param name="contentType">Loại nội dung (MIME type) của file.</param>
    /// <param name="folder">Thư mục lưu trữ (tùy chọn).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result chứa FileStorageResultDto với đường dẫn đến file đã lưu.</returns>
    Task<Result<FileStorageResultDto>> SaveFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null, CancellationToken cancellationToken = default);


    /// <summary>
    /// Lấy file từ bộ nhớ.
    /// </summary>
    /// <param name="filePath">Đường dẫn đến file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stream của file.</returns>
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa file khỏi bộ nhớ.
    /// </summary>
    /// <param name="filePath">Đường dẫn đến file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy URL truy cập công khai đến file.
    /// </summary>
    /// <param name="filePath">Đường dẫn đến file.</param>
    /// <returns>URL truy cập công khai.</returns>
    string GetFileUrl(string filePath);
}
