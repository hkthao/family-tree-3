using backend.Application.Common.Models;

namespace backend.Application.Identity.Queries.GetUserByUsernameOrEmail;

/// <summary>
/// Truy vấn để lấy thông tin người dùng dựa trên tên người dùng hoặc email.
/// </summary>
public record GetUserByUsernameOrEmailQuery : IRequest<Result<UserDto>>
{
    /// <summary>
    /// Tên người dùng hoặc email để tìm kiếm.
    /// </summary>
    public string? UsernameOrEmail { get; init; }
}
