using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Identity.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IAuthProvider _authProvider;
    private readonly IUser _user;

    public UpdateUserProfileCommandHandler(IAuthProvider authProvider, IUser user)
    {
        _authProvider = authProvider;
        _user = user;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // Security check: Only allow users to update their own profile
        if (_user.Id != request.Id)
        {
            return Result.Failure("Unauthorized: You can only update your own profile.", "Forbidden");
        }

        // Call the AuthProvider to update the user profile
        var result = await _authProvider.UpdateUserProfileAsync(request.Id, request);

        return result;
    }
}