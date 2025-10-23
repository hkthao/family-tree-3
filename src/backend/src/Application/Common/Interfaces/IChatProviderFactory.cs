using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà máy tạo các nhà cung cấp dịch vụ trò chuyện AI.
/// </summary>
public interface IChatProviderFactory
{
    /// <summary>
    /// Lấy một thể hiện của IChatProvider dựa trên loại nhà cung cấp AI được chỉ định.
    /// </summary>
    /// <param name="provider">Loại nhà cung cấp AI trò chuyện.</param>
    /// <returns>Một thể hiện của IChatProvider.</returns>
    IChatProvider GetProvider(ChatAIProvider provider);
}
