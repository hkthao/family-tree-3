using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Common.Services;

public class FamilyAuthorizationService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;

    public FamilyAuthorizationService(IApplicationDbContext context, IUser user, IAuthorizationService authorizationService)
    {
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
    }

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

        if (!(_user.Roles != null && _user.Roles.Contains(SystemRole.Admin.ToString())) &&
            !family.FamilyUsers.Any(fu => fu.UserProfileId == currentUserProfile!.Id && fu.Role == FamilyRole.Manager))
        {
            return Result<Family>.Failure($"User is not authorized to manage family {family.Name}.", "Authorization");
        }

        return Result<Family>.Success(family);
    }
}
