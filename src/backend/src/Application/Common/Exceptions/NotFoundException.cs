namespace backend.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ được ném ra khi một thực thể không được tìm thấy.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp NotFoundException.
    /// </summary>
    public NotFoundException() : base() { }

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp NotFoundException với một thông báo lỗi cụ thể.
    /// </summary>
    /// <param name="message">Thông báo lỗi mô tả nguyên nhân của ngoại lệ.</param>
    public NotFoundException(string message) : base(message) { }

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp NotFoundException với một thông báo lỗi cụ thể
    /// và một tham chiếu đến ngoại lệ bên trong là nguyên nhân của ngoại lệ này.
    /// </summary>
    /// <param name="message">Thông báo lỗi mô tả nguyên nhân của ngoại lệ.</param>
    /// <param name="innerException">Ngoại lệ là nguyên nhân của ngoại lệ hiện tại.</param>
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp NotFoundException với tên thực thể và khóa không tìm thấy.
    /// </summary>
    /// <param name="name">Tên của thực thể không tìm thấy.</param>
    /// <param name="key">Khóa của thực thể không tìm thấy.</param>
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }
}
