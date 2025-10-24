using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà cung cấp dịch vụ tạo embedding.
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Lấy tên của nhà cung cấp embedding.
    /// </summary>
    string ProviderName { get; }
    /// <summary>
    /// Lấy độ dài văn bản tối đa mà nhà cung cấp embedding có thể xử lý.
    /// </summary>
    int MaxTextLength { get; }
    /// <summary>
    /// Tạo embedding cho một đoạn văn bản.
    /// </summary>
    /// <param name="text">Đoạn văn bản cần tạo embedding.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result chứa mảng double biểu diễn embedding hoặc thông báo lỗi.</returns>
    Task<Result<double[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    int EmbeddingDimension { get; }
}
