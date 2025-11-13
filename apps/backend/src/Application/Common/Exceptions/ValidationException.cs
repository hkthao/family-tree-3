using FluentValidation.Results;

namespace backend.Application.Common.Exceptions;

/// <summary>
/// Ngoại lệ được ném ra khi có một hoặc nhiều lỗi xác thực xảy ra.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp ValidationException.
    /// </summary>
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp ValidationException với một thông báo lỗi cụ thể.
    /// </summary>
    /// <param name="message">Thông báo lỗi.</param>
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp ValidationException với các lỗi xác thực cụ thể.
    /// </summary>
    /// <param name="failures">Danh sách các lỗi xác thực.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    /// <summary>
    /// Lấy một từ điển chứa các lỗi xác thực, với khóa là tên thuộc tính và giá trị là mảng các thông báo lỗi.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }
}
