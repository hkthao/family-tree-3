using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà máy tạo các kho lưu trữ vector.
/// </summary>
public interface IVectorStoreFactory
{
    /// <summary>
    /// Tạo một thể hiện của IVectorStore dựa trên loại nhà cung cấp kho vector được chỉ định.
    /// </summary>
    /// <param name="provider">Loại nhà cung cấp kho vector.</param>
    /// <returns>Một thể hiện của IVectorStore.</returns>
    IVectorStore CreateVectorStore(VectorStoreProviderType provider);
}
