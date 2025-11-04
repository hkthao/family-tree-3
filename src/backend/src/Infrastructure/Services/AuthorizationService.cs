using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Infrastructure.Services;

public class AuthorizationService(ICurrentUser user, IApplicationDbContext context) : IAuthorizationService
{
    private readonly ICurrentUser _user = user;
    private readonly IApplicationDbContext _context = context;

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

    public bool HasFamilyRole(Guid familyId, FamilyRole requiredRole)
    {
        if (IsAdmin()) return true; // Admin bypasses family-specific role checks

        var familyUser = _context.FamilyUsers.FirstOrDefault(fu => fu.FamilyId == familyId && fu.UserId == _user.UserId);
        if (familyUser == null) return false;

        return familyUser.Role <= requiredRole; // Assuming enum values are ordered by privilege
    }
}
