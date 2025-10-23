using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà máy tạo các nhà cung cấp dịch vụ lưu trữ tệp.
/// </summary>
public interface IFileStorageFactory
{
    /// <summary>
    /// Tạo một thể hiện của IFileStorage dựa trên nhà cung cấp lưu trữ được chỉ định.
    /// </summary>
    /// <param name="provider">Nhà cung cấp lưu trữ.</param>
    /// <returns>Một thể hiện của IFileStorage.</returns>
    IFileStorage CreateFileStorage(StorageProvider provider);
}
