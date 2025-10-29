namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà cung cấp cấu hình, cho phép truy xuất các giá trị cấu hình.
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Lấy giá trị cấu hình theo khóa, với giá trị mặc định được cung cấp.
    /// </summary>
    /// <typeparam name="T">Kiểu của giá trị cấu hình.</typeparam>
    /// <param name="key">Khóa của giá trị cấu hình.</param>
    /// <param name="defaultValue">Giá trị mặc định sẽ được trả về nếu không tìm thấy khóa.</param>
    /// <returns>Giá trị cấu hình hoặc giá trị mặc định.</returns>
    Task<T?> GetValue<T>(string key, T defaultValue);
    /// <summary>
    /// Lấy giá trị cấu hình theo khóa.
    /// </summary>
    /// <typeparam name="T">Kiểu của giá trị cấu hình.</typeparam>
    /// <param name="key">Khóa của giá trị cấu hình.</param>
    /// <returns>Giá trị cấu hình hoặc null nếu không tìm thấy.</returns>
    Task<T?> GetValue<T>(string key);
}
