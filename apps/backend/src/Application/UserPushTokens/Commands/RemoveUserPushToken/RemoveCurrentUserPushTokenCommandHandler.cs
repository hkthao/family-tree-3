using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.UserPushTokens.Commands.RemoveCurrentUserPushToken;

public class RemoveCurrentUserPushTokenCommandHandler : IRequestHandler<RemoveCurrentUserPushTokenCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public RemoveCurrentUserPushTokenCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RemoveCurrentUserPushTokenCommand request, CancellationToken cancellationToken)
    {
        // Check if the current user is authenticated
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure("User is not authenticated.", "Authentication");
        }
        // Ensure that the UserId is available
        if (_currentUser.UserId == Guid.Empty)
        {
            return Result.Failure("User ID is not available.", "Authentication");
        }

        var userPushToken = await _context.UserPushTokens
            .FirstOrDefaultAsync(
                t => t.DeviceId == request.DeviceId &&
                     t.ExpoPushToken == request.ExpoPushToken &&
                     t.UserId == _currentUser.UserId, // Use _currentUser.UserId
                cancellationToken);

        if (userPushToken == null)
        {
            return Result.Failure("User push token not found or does not belong to the current user.");
        }

        _context.UserPushTokens.Remove(userPushToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
