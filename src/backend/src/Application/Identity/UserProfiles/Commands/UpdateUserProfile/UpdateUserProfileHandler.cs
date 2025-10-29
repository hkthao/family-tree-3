using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;

namespace backend.Application.Identity.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // Retrieve UserProfile from DB to get ExternalId
        if (!Guid.TryParse(request.Id, out var userId))
        {
            return Result.Failure(ErrorMessages.InvalidUserIdFormat, ErrorSources.Validation);
        }
        var userProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(up => up.Id == userId, cancellationToken);

        if (userProfile == null)
        {
            return Result.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        // Update existing UserProfile in local DB
        if (request.Name != null) userProfile.Name = request.Name;
        if (request.Email != null) userProfile.Email = request.Email;
        if (request.Avatar != null) userProfile.Avatar = request.Avatar;
        // Other fields like Picture or UserMetadata are managed by Auth0 directly

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
