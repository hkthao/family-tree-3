
namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho thông tin người dùng hiện tại.
/// </summary>
public interface IUser
{
    /// <summary>
    /// Lấy ID của người dùng hiện tại.
    /// </summary>
    string? Id { get; }
    /// <summary>
    /// Lấy danh sách các vai trò của người dùng hiện tại.
    /// </summary>
    List<string>? Roles { get; }
}
