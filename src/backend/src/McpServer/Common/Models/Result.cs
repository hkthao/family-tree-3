namespace McpServer.Common.Models;

/// <summary>
/// Đại diện cho kết quả của một thao tác có thể thành công hoặc thất bại, có chứa giá trị trả về.
/// </summary>
/// <typeparam name="T">Kiểu của giá trị trả về khi thao tác thành công.</typeparam>
public class Result<T>
{
    /// <summary>
    /// Cho biết thao tác có thành công hay không.
    /// </summary>
    public bool IsSuccess { get; private set; }
    /// <summary>
    /// Thông báo lỗi nếu thao tác thất bại.
    /// </summary>
    public string? Error { get; private set; }
    /// <summary>
    /// Nguồn gốc của lỗi (ví dụ: Validation, Database, Exception, ExternalService).
    /// </summary>
    public string? ErrorSource { get; private set; } // e.g., Validation, Database, Exception, ExternalService
    /// <summary>
    /// Giá trị trả về khi thao tác thành công.
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// Tạo một kết quả thành công với giá trị được cung cấp.
    /// </summary>
    /// <param name="value">Giá trị trả về.</param>
    /// <returns>Một thể hiện của Result<T> biểu thị thành công.</returns>
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    /// <summary>
    /// Tạo một kết quả thất bại với thông báo lỗi và nguồn gốc lỗi.
    /// </summary>
    /// <param name="error">Thông báo lỗi.</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "Unknown".</param>
    /// <returns>Một thể hiện của Result<T> biểu thị thất bại.</returns>
    public static Result<T> Failure(string error, string errorSource = "Unknown") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource };
}

/// <summary>
/// Đại diện cho kết quả của một thao tác có thể thành công hoặc thất bại, không chứa giá trị trả về.
/// </summary>
public class Result
{
    /// <summary>
    /// Cho biết thao tác có thành công hay không.
    /// </summary>
    public bool IsSuccess { get; private set; }
    /// <summary>
    /// Thông báo lỗi nếu thao tác thất bại.
    /// </summary>
    public string? Error { get; private set; }
    /// <summary>
    /// Nguồn gốc của lỗi.
    /// </summary>
    public string? ErrorSource { get; private set; }

    /// <summary>
    /// Tạo một kết quả thành công.
    /// </summary>
    /// <returns>Một thể hiện của Result biểu thị thành công.</returns>
    public static Result Success() => new() { IsSuccess = true };
    /// <summary>
    /// Tạo một kết quả thất bại với thông báo lỗi và nguồn gốc lỗi.
    /// </summary>
    /// <param name="error">Thông báo lỗi.</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "Unknown".</param>
    /// <returns>Một thể hiện của Result biểu thị thất bại.</returns>
    public static Result Failure(string error, string errorSource = "Unknown") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource };

    internal static Result Failure(string[] strings)
    {
        throw new NotImplementedException();
    }
}
