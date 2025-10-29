using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà máy tạo các nhà cung cấp dịch vụ tạo embedding.
/// </summary>
public interface IEmbeddingProviderFactory
{
    /// <summary>
    /// Lấy một thể hiện của IEmbeddingProvider dựa trên loại nhà cung cấp AI embedding được chỉ định.
    /// </summary>
    /// <param name="provider">Loại nhà cung cấp AI embedding.</param>
    /// <returns>Một thể hiện của IEmbeddingProvider.</returns>
    IEmbeddingProvider GetProvider(EmbeddingAIProvider provider);
}
