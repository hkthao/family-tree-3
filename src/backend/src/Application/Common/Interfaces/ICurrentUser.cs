using System;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho dịch vụ cung cấp thông tin về người dùng hiện tại.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Lấy ID của người dùng hiện tại (từ entity User).
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Lấy ID của hồ sơ người dùng hiện tại đang hoạt động (từ entity UserProfile).
    /// Có thể null nếu người dùng chưa có hồ sơ hoặc hồ sơ không được chọn.
    /// </summary>
    Guid? ProfileId { get; }

    /// <summary>
    /// Lấy địa chỉ email của người dùng hiện tại.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Lấy tên hiển thị của người dùng hiện tại.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Lấy danh sách các vai trò của người dùng hiện tại.
    /// </summary>
    List<string>? Roles { get; }

    /// <summary>
    /// Kiểm tra xem người dùng hiện tại có được xác thực hay không.
    /// </summary>
    bool IsAuthenticated { get; }
}