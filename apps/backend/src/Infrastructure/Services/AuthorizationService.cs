using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Infrastructure.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly ICurrentUser _user;
    private readonly IApplicationDbContext _context;

    public AuthorizationService(ICurrentUser user, IApplicationDbContext context)
    {
        _user = user;
        _context = context;
    }

    public Task<Result> AuthorizeAsync(string role)
    {
        // For now, only 'Admin' role is supported through this method
        if (role == AppRoles.Administrator)
        {
            if (IsAdmin())
            {
                return Task.FromResult(Result.Success());
            }
            return Task.FromResult(Result.Unauthorized("Người dùng không có quyền quản trị viên."));
        }
        // If other roles need to be supported, they can be added here
        return Task.FromResult(Result.Failure($"Vai trò '{role}' không được hỗ trợ để ủy quyền."));
    }

    public bool IsAdmin()
    {
        return _user.Roles != null && _user.Roles.Contains("Admin");
    }

    public bool CanAccessFamily(Guid familyId)
    {
        if (IsAdmin()) return true; // Admin bypasses family-specific role checks
        return _context.FamilyUsers.Any(fu => fu.FamilyId == familyId && fu.UserId == _user.UserId);
    }

    public bool CanManageFamily(Guid familyId)
    {
        if (IsAdmin()) return true; // Admin bypasses family-specific role checks
        return _context.FamilyUsers.Any(fu => fu.FamilyId == familyId && fu.UserId == _user.UserId && fu.Role == FamilyRole.Manager);
    }

    public bool CanViewFamily(Guid familyId)
    {
        if (IsAdmin()) return true; // Admin can view any family
        // A user can view a family if they are either a Manager or a Viewer
        return _context.FamilyUsers.Any(fu => fu.FamilyId == familyId && fu.UserId == _user.UserId && (fu.Role == FamilyRole.Manager || fu.Role == FamilyRole.Viewer));
    }

    public bool HasFamilyRole(Guid familyId, FamilyRole requiredRole)
    {
        if (IsAdmin()) return true; // Admin bypasses family-specific role checks

        var familyUser = _context.FamilyUsers.FirstOrDefault(fu => fu.FamilyId == familyId && fu.UserId == _user.UserId);
        if (familyUser == null) return false;

        return familyUser.Role <= requiredRole; // Assuming enum values are ordered by privilege
    }
}
