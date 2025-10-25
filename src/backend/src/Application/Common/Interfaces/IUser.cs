
namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho thông tin người dùng hiện tại.
/// </summary>
public interface IUser
{
    /// <summary>
    /// Lấy ID hồ sơ người dùng nội bộ (UserProfileId) của người dùng hiện tại.
    /// </summary>
    Guid? Id { get; }

    /// <summary>
    /// Lấy ID từ nhà cung cấp xác thực bên ngoài của người dùng hiện tại (ví dụ: Google ID, Firebase UID).
    /// </summary>
    string? ExternalId { get; }

    /// <summary>
    /// Lấy địa chỉ email của người dùng hiện tại.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Lấy tên hiển thị của người dùng hiện tại.
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// Kiểm tra xem người dùng hiện tại đã được xác thực hay chưa.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Lấy danh sách các vai trò của người dùng hiện tại.
    /// </summary>
    List<string>? Roles { get; }
}
