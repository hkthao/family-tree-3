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
}
