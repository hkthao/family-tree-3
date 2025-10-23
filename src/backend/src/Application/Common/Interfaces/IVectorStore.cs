using backend.Application.AI.VectorStore;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho kho lưu trữ vector.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Thêm hoặc cập nhật một embedding vào kho vector.
    /// </summary>
    /// <param name="embedding">Embedding cần thêm hoặc cập nhật.</param>
    /// <param name="metadata">Các siêu dữ liệu liên quan đến embedding.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    Task UpsertAsync(List<double> embedding, Dictionary<string, string> metadata, CancellationToken cancellationToken = default);
    /// <summary>
    /// Thêm hoặc cập nhật một embedding vào kho vector trong một collection cụ thể.
    /// </summary>
    /// <param name="embedding">Embedding cần thêm hoặc cập nhật.</param>
    /// <param name="metadata">Các siêu dữ liệu liên quan đến embedding.</param>
    /// <param name="collectionName">Tên của collection để lưu trữ embedding.</param>
    /// <param name="embeddingDimension">Kích thước của embedding.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    Task UpsertAsync(List<double> embedding, Dictionary<string, string> metadata, string collectionName, int embeddingDimension, CancellationToken cancellationToken = default); // New overload
    /// <summary>
    /// Truy vấn kho vector để tìm các embedding tương tự.
    /// </summary>
    /// <param name="queryEmbedding">Embedding truy vấn.</param>
    /// <param name="topK">Số lượng kết quả hàng đầu cần trả về.</param>
    /// <param name="metadataFilter">Bộ lọc siêu dữ liệu tùy chọn.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Danh sách các kết quả truy vấn từ kho vector.</returns>
    Task<List<VectorStoreQueryResult>> QueryAsync(double[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, CancellationToken cancellationToken = default);
    /// <summary>
    /// Truy vấn kho vector trong một collection cụ thể để tìm các embedding tương tự.
    /// </summary>
    /// <param name="queryEmbedding">Embedding truy vấn.</param>
    /// <param name="topK">Số lượng kết quả hàng đầu cần trả về.</param>
    /// <param name="metadataFilter">Bộ lọc siêu dữ liệu tùy chọn.</param>
    /// <param name="collectionName">Tên của collection để truy vấn.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Danh sách các kết quả truy vấn từ kho vector.</returns>
    Task<List<VectorStoreQueryResult>> QueryAsync(double[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, string collectionName, CancellationToken cancellationToken = default); // New overload
}
