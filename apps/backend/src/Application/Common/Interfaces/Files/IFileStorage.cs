using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces.Files;

/// <summary>
/// Định nghĩa giao diện cho các hoạt động lưu trữ tệp.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Tải lên một tệp lên nhà cung cấp lưu trữ đã cấu hình.
    /// </summary>
    /// <param name="fileStream">Luồng của tệp cần tải lên.</param>
    /// <param name="fileName">Tên gốc của tệp.</param>
    /// <param name="contentType">Loại nội dung của tệp.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result chứa URL của tệp đã tải lên khi thành công, hoặc lỗi khi thất bại.</returns>
    Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Xóa một tệp khỏi nhà cung cấp lưu trữ đã cấu hình.
    /// </summary>
    /// <param name="url">URL của tệp cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result cho biết thành công hay thất bại.</returns>
    Task<Result> DeleteFileAsync(string url, CancellationToken cancellationToken);
}
