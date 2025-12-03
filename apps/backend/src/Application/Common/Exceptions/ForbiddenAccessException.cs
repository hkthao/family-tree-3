namespace backend.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ được ném ra khi người dùng cố gắng truy cập một tài nguyên mà họ không có quyền.
/// </summary>
public class ForbiddenAccessException : Exception
{
    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp ForbiddenAccessException.
    /// </summary>
    public ForbiddenAccessException() : base() { }

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp ForbiddenAccessException với một thông báo lỗi cụ thể.
    /// </summary>
    /// <param name="message">Thông báo mô tả lỗi.</param>
    public ForbiddenAccessException(string message) : base(message) { }
}
