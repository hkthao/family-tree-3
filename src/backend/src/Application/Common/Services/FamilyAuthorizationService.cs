using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Common.Services;

/// <summary>
/// Cung cấp các dịch vụ ủy quyền liên quan đến quyền truy cập gia đình.
/// </summary>
public class FamilyAuthorizationService(IApplicationDbContext context, IUser user, IAuthorizationService authorizationService)
{
    /// <summary>
    /// Ngữ cảnh cơ sở dữ liệu ứng dụng.
    /// </summary>
    private readonly IApplicationDbContext _context = context;
    /// <summary>
    /// Thông tin người dùng hiện tại.
    /// </summary>
    private readonly IUser _user = user;
    /// <summary>
    /// Dịch vụ ủy quyền chung.
    /// </summary>
    private readonly IAuthorizationService _authorizationService = authorizationService;

    /// <summary>
    /// Ủy quyền truy cập vào một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình cần kiểm tra quyền truy cập.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result<Family> cho biết liệu người dùng có quyền truy cập vào gia đình hay không. Nếu thành công, chứa thông tin gia đình.</returns>
    public virtual async Task<Result<Family>> AuthorizeFamilyAccess(Guid? familyId, CancellationToken cancellationToken)
    {
        if (!familyId.HasValue)
        {
            return Result<Family>.Failure("Family ID is required.");
        }

        var family = await _context.Families
            .Include(f => f.FamilyUsers)
            .FirstOrDefaultAsync(f => f.Id == familyId.Value, cancellationToken);

        if (family == null)
        {
            return Result<Family>.Failure($"Family with ID {familyId.Value} not found.");
        }

        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);

        return !(_user.Roles != null && _user.Roles.Contains(SystemRole.Admin.ToString())) &&
            !family.FamilyUsers.Any(fu => fu.UserProfileId == currentUserProfile!.Id && fu.Role == FamilyRole.Manager)
            ? Result<Family>.Failure($"User is not authorized to manage family {family.Name}.", "Authorization")
            : Result<Family>.Success(family);
    }
}
