using backend.Domain.Enums; // Assuming RestorationStatus enum is defined here or similar

namespace backend.Application.Common.Models;

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
    /// Mã trạng thái HTTP liên quan đến kết quả.
    /// </summary>
    public int StatusCode { get; private set; } = 200; // Default to 200 OK
    /// <summary>
    /// Thông báo lỗi nếu thao tác thất bại.
    /// </summary>
    public string? Error { get; private set; }
    /// <summary>
    /// Nguồn gốc của lỗi (ví dụ: Validation, Database, Exception, ExternalService).
    /// </summary>
    public string? ErrorSource { get; private set; } // e.g., Validation, Database, Exception, ExternalService
    /// <summary>
    /// Các lỗi xác thực chi tiết, nếu có. Key là tên trường, Value là mảng các lỗi cho trường đó.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; private set; }
    /// <summary>
    /// Giá trị trả về khi thao tác thành công.
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// Tạo một kết quả thành công với giá trị được cung cấp.
    /// </summary>
    /// <param name="value">Giá trị trả về.</param>
    /// <returns>Một thể hiện của Result<T> biểu thị thành công.</returns>
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value, StatusCode = 200 };
    /// <summary>
    /// Tạo một kết quả thất bại với thông báo lỗi và nguồn gốc lỗi.
    /// </summary>
    /// <param name="error">Thông báo lỗi.</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "Unknown".</param>
    /// <returns>Một thể hiện của Result<T> biểu thị thất bại.</returns>
    public static Result<T> Failure(string error, string errorSource = "Unknown") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource, StatusCode = 400 }; // Default to 400 Bad Request

    /// <summary>
    /// Tạo một kết quả thất bại với các lỗi xác thực chi tiết.
    /// </summary>
    /// <param name="validationErrors">Từ điển các lỗi xác thực.</param>
    /// <returns>Một thể hiện của Result<T> biểu thị thất bại do lỗi xác thực.</returns>
    public static Result<T> Failure(Dictionary<string, string[]> validationErrors) =>
        new()
        { IsSuccess = false, Error = "One or more validation errors occurred.", ErrorSource = "Validation", StatusCode = 400, ValidationErrors = validationErrors };

    /// <summary>
    /// Tạo một kết quả cấm truy cập (Forbidden) với thông báo lỗi và mã trạng thái 403.
    /// </summary>
    /// <param name="error">Thông báo lỗi. Mặc định là "Forbidden".</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "Authorization".</param>
    /// <returns>Một thể hiện của Result<T> biểu thị bị cấm truy cập.</returns>
    public static Result<T> Forbidden(string error = "Forbidden", string errorSource = "Authorization") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource, StatusCode = 403 };

    /// <summary>
    /// Tạo một kết quả không tìm thấy (Not Found) với thông báo lỗi và mã trạng thái 404.
    /// </summary>
    /// <param name="error">Thông báo lỗi. Mặc định là "Not Found".</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "NotFound".</param>
    /// <returns>Một thể hiện của Result<T> biểu thị không tìm thấy.</returns>
    public static Result<T> NotFound(string error = "NotFound", string errorSource = "NotFound") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource, StatusCode = 404 };

    /// <summary>
    /// Tạo một kết quả xung đột (Conflict) với thông báo lỗi và mã trạng thái 409.
    /// </summary>
    /// <param name="error">Thông báo lỗi. Mặc định là "Conflict".</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "Conflict".</param>
    /// <returns>Một thể hiện của Result<T> biểu thị xung đột.</returns>
    public static Result<T> Conflict(string error = "Conflict", string errorSource = "Conflict") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource, StatusCode = 409 };
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
    /// Mã trạng thái HTTP liên quan đến kết quả.
    /// </summary>
    public int StatusCode { get; private set; } = 400; // Default to 400 Bad Request
    /// <summary>
    /// Thông báo lỗi nếu thao tác thất bại.
    /// </summary>
    public string? Error { get; private set; }
    /// <summary>
    /// Nguồn gốc của lỗi.
    /// </summary>
    public string? ErrorSource { get; private set; }

    /// <summary>
    /// Các lỗi xác thực chi tiết, nếu có. Key là tên trường, Value là mảng các lỗi cho trường đó.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; private set; }

    /// <summary>
    /// Tạo một kết quả thành công.
    /// </summary>
    /// <returns>Một thể hiện của Result biểu thị thành công.</returns>
    public static Result Success() => new() { IsSuccess = true, StatusCode = 200 };
    /// <summary>
    /// Tạo một kết quả thất bại với thông báo lỗi và nguồn gốc lỗi.
    /// </summary>
    /// <param name="error">Thông báo lỗi.</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "Unknown".</param>
    /// <returns>Một thể hiện của Result biểu thị thất bại.</returns>
    public static Result Failure(string error, string errorSource = "Unknown") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource, StatusCode = 400 };

    /// <summary>
    /// Tạo một kết quả thất bại với các lỗi xác thực chi tiết.
    /// </summary>
    /// <param name="validationErrors">Từ điển các lỗi xác thực.</param>
    /// <returns>Một thể hiện của Result biểu thị thất bại do lỗi xác thực.</returns>
    public static Result Failure(Dictionary<string, string[]> validationErrors) =>
        new()
        { IsSuccess = false, Error = "One or more validation errors occurred.", ErrorSource = "Validation", StatusCode = 400, ValidationErrors = validationErrors };

    /// <summary>
    /// Tạo một kết quả không được phép (Unauthorized) với thông báo lỗi và mã trạng thái 401.
    /// </summary>
    /// <param name="error">Thông báo lỗi. Mặc định là "Unauthorized".</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "Authorization".</param>
    /// <returns>Một thể hiện của Result biểu thị không được phép.</returns>
    public static Result Unauthorized(string error = "Unauthorized", string errorSource = "Authorization") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource, StatusCode = 401 };

    /// <summary>
    /// Tạo một kết quả cấm truy cập (Forbidden) với thông báo lỗi.
    /// </summary>
    /// <param name="error">Thông báo lỗi. Mặc định là "Forbidden".</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "Authorization".</param>
    /// <returns>Một thể hiện của Result biểu thị bị cấm truy cập.</returns>
    public static Result Forbidden(string error = "Forbidden", string errorSource = "Authorization") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource, StatusCode = 403 };

    /// <summary>
    /// Tạo một kết quả không tìm thấy (Not Found) với thông báo lỗi và mã trạng thái 404.
    /// </summary>
    /// <param name="error">Thông báo lỗi. Mặc định là "Not Found".</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "NotFound".</param>
    /// <returns>Một thể hiện của Result biểu thị không tìm thấy.</returns>
    public static Result NotFound(string error = "NotFound", string errorSource = "NotFound") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource, StatusCode = 404 };

    /// <summary>
    /// Tạo một kết quả xung đột (Conflict) với thông báo lỗi.
    /// </summary>
    /// <param name="error">Thông báo lỗi. Mặc định là "Conflict".</param>
    /// <param name="errorSource">Nguồn gốc của lỗi. Mặc định là "Conflict".</param>
    /// <returns>Một thể hiện của Result biểu thị xung đột.</returns>
    public static Result Conflict(string error = "Conflict", string errorSource = "Conflict") =>
        new()
        { IsSuccess = false, Error = error, ErrorSource = errorSource, StatusCode = 409 };
}


