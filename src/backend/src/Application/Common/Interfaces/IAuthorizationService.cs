namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho dịch vụ ủy quyền, cung cấp các phương thức để kiểm tra quyền truy cập và vai trò của người dùng.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Kiểm tra xem người dùng hiện tại có phải là quản trị viên hay không.
    /// </summary>
    /// <returns>True nếu người dùng là quản trị viên, ngược lại là false.</returns>
    bool IsAdmin();

    /// <summary>
    /// Truy xuất UserProfile của người dùng hiện tại, bao gồm các liên kết gia đình của họ.
    /// </summary>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>UserProfile nếu tìm thấy, ngược lại là null.</returns>
    Task<UserProfile?> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem người dùng hiện tại có quyền truy cập (Quản lý hoặc Người xem) vào một gia đình cụ thể hay không.
    /// </summary>
    /// <param name="familyId">ID của gia đình cần kiểm tra quyền truy cập.</param>
    /// <param name="userProfile">Hồ sơ người dùng hiện tại.</param>
    /// <returns>True nếu người dùng có quyền truy cập, ngược lại là false.</returns>
    bool CanAccessFamily(Guid familyId, UserProfile userProfile);

    /// <summary>
    /// Kiểm tra xem người dùng hiện tại có quyền quản lý (vai trò Quản lý) đối với một gia đình cụ thể hay không.
    /// </summary>
    /// <param name="familyId">ID của gia đình cần kiểm tra quyền quản lý.</param>
    /// <param name="userProfile">Hồ sơ người dùng hiện tại.</param>
    /// <returns>True nếu người dùng có thể quản lý gia đình, ngược lại là false.</n
    bool CanManageFamily(Guid familyId, UserProfile userProfile);

    /// <summary>
    /// Kiểm tra xem người dùng hiện tại có một vai trò cụ thể trong một gia đình hay không.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="userProfile">Hồ sơ người dùng hiện tại.</param>
    /// <param name="requiredRole">Vai trò tối thiểu được yêu cầu.</param>
    /// <returns>True nếu người dùng có vai trò yêu cầu hoặc cao hơn, ngược lại là false.</returns>
    bool HasFamilyRole(Guid familyId, UserProfile userProfile, FamilyRole requiredRole);
}
