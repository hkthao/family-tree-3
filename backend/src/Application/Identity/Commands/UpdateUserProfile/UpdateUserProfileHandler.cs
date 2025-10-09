using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Specifications;
using Microsoft.EntityFrameworkCore;

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
        if (_user.Id != request.Id)
        {
            return Result.Failure("Unauthorized: You can only update your own profile.", "Forbidden");
        }

        // Call the AuthProvider to update the user profile
        var authProviderResult = await _authProvider.UpdateUserProfileAsync(request.Id, request);
        if (!authProviderResult.IsSuccess)
        {
            return authProviderResult;
        }

        // Update or create UserProfile in local DB
        var userProfile = await _context.UserProfiles
            .WithSpecification(new UserProfileByAuth0IdSpec(request.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (userProfile == null)
        {
            // Create new UserProfile
            userProfile = new UserProfile
            {
                Auth0UserId = request.Id,
                Email = request.Email ?? "", // Assuming email is always provided or can be null
                Name = request.Name ?? "",   // Assuming name is always provided or can be null
            };
            _context.UserProfiles.Add(userProfile);
        }
        else
        {
            // Update existing UserProfile
            if (request.Name != null) userProfile.Name = request.Name;
            if (request.Email != null) userProfile.Email = request.Email;
            // Other fields like Picture or UserMetadata are managed by Auth0 directly
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
