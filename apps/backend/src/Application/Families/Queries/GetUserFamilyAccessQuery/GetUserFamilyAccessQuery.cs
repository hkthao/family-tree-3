using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.GetUserFamilyAccessQuery;

/// <summary>
/// Truy vấn để lấy tất cả các gia đình mà người dùng hiện tại có quyền truy cập (Manager hoặc Viewer).
/// </summary>
public record GetUserFamilyAccessQuery : IRequest<Result<List<FamilyAccessDto>>>
{
    // Không cần thêm thuộc tính UserId ở đây vì UserID sẽ được lấy từ ICurrentUserService trong Handler.
    // Nếu query cần tham số, chúng ta sẽ thêm chúng ở đây.
}
