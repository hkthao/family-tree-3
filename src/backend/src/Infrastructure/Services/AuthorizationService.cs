using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services;

public class AuthorizationService(IUser user, IApplicationDbContext context) : IAuthorizationService
{
    private readonly IUser _user = user;
    private readonly IApplicationDbContext _context = context;

    public bool IsAdmin()
    {
        return _user.Roles != null && _user.Roles.Contains("Admin");
    }

    public async Task<UserProfile?> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
    {
        return string.IsNullOrEmpty(_user.Id)
            ? null
            : await _context.UserProfiles
            .WithSpecification(new UserProfileByAuth0IdSpec(_user.Id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public bool CanAccessFamily(Guid familyId, UserProfile userProfile)
    {
        return userProfile.FamilyUsers.Any(fu => fu.FamilyId == familyId);
    }

    public bool CanManageFamily(Guid familyId, UserProfile userProfile)
    {
        return userProfile.FamilyUsers.Any(fu => fu.FamilyId == familyId && fu.Role == FamilyRole.Manager);
    }

    public bool HasFamilyRole(Guid familyId, UserProfile userProfile, FamilyRole requiredRole)
    {
        if (IsAdmin()) return true; // Admin bypasses family-specific role checks

        var familyUser = userProfile.FamilyUsers.FirstOrDefault(fu => fu.FamilyId == familyId);
        if (familyUser == null) return false;

        return familyUser.Role <= requiredRole; // Assuming enum values are ordered by privilege
    }
}
