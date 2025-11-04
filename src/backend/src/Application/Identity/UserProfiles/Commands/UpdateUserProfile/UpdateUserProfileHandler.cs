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
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Profile != null && u.Profile.Id == request.Id, cancellationToken);

        if (user == null || user.Profile == null)
        {
            return Result.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        user.UpdateProfile(
            user.Profile.ExternalId, // ExternalId should not be updated here
            request.Email ?? user.Profile.Email,
            (request.FirstName != null || request.LastName != null) ? $"{request.FirstName ?? user.Profile.FirstName} {request.LastName ?? user.Profile.LastName}".Trim() : user.Profile.Name,
            request.FirstName ?? user.Profile.FirstName ?? string.Empty, // Provide empty string if null
            request.LastName ?? user.Profile.LastName ?? string.Empty, // Provide empty string if null
            request.Phone ?? user.Profile.Phone ?? string.Empty, // Provide empty string if null
            request.Avatar ?? user.Profile.Avatar ?? string.Empty // Provide empty string if null
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
