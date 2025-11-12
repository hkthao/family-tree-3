namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà cung cấp cấu hình ứng dụng.
/// </summary>
public interface IConfigProvider
{
    /// <summary>
    /// Lấy một phần cấu hình và ánh xạ nó sang một đối tượng kiểu T.
    /// </summary>
    /// <typeparam name="T">Kiểu của đối tượng cấu hình.</typeparam>
    /// <returns>Đối tượng cấu hình đã được ánh xạ, hoặc một đối tượng T mới nếu không tìm thấy cấu hình.</returns>
    T GetSection<T>() where T : new();
}
