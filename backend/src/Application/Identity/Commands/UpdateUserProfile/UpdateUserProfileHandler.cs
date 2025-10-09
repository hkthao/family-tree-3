using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Identity.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IAuthProvider _authProvider;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public UpdateUserProfileCommandHandler(IAuthProvider authProvider, IUser user, IApplicationDbContext context)
    {
        _authProvider = authProvider;
        _user = user;
        _context = context;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // Security check: Only allow users to update their own profile
        // if (_user.Id != request.ExternalId)
        // {
        //     return Result.Failure("Unauthorized: You can only update your own profile.", "Forbidden");
        // }

        // Retrieve UserProfile from DB to get ExternalId
        if (!Guid.TryParse(request.Id, out var userId))
        {
            return Result.Failure("Invalid user ID format.", "BadRequest");
        }
        var userProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(up => up.Id == userId, cancellationToken);

        if (userProfile == null)
        {
            return Result.Failure("User profile not found.", "NotFound");
        }

        // Call the AuthProvider to update the user profile using ExternalId
        // var authProviderResult = await _authProvider.UpdateUserProfileAsync(userProfile.ExternalId, request);
        // if (authProviderResult == null)
        // {
        //     return Result.Failure("An error occurred while updating the user profile.", "InternalServerError");
        // }
        // if (!authProviderResult.IsSuccess)
        // {
        //     return authProviderResult;
        // }

        // Update existing UserProfile in local DB
        if (request.Name != null) userProfile.Name = request.Name;
        if (request.Email != null) userProfile.Email = request.Email;
        if (request.Avatar != null) userProfile.Avatar = request.Avatar;
        // Other fields like Picture or UserMetadata are managed by Auth0 directly

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
