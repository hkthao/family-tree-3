using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Tải file lên bộ nhớ.
    /// </summary>
    /// <param name="fileStream">Stream của file cần tải lên.</param>
    /// <param name="fileName">Tên file.</param>
    /// <param name="folder">Thư mục lưu trữ (tùy chọn).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Đường dẫn đến file đã tải lên.</returns>
    Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string? folder = null, CancellationToken cancellationToken = default);

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
