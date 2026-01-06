using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Notifications.Commands.SaveExpoPushToken;
using backend.Application.Notifications.Commands.SyncSubscriber;
using backend.Application.Notifications.DTOs;
using backend.Domain.Entities;

namespace backend.Application.UserPushTokens.Commands.SyncUserPushToken;

public class SyncCurrentUserPushTokenCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IMediator mediator) : IRequestHandler<SyncCurrentUserPushTokenCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IMediator _mediator = mediator;

    public async Task<Result> Handle(SyncCurrentUserPushTokenCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        await _mediator.Send(new SyncSubscriberCommand(_currentUser.UserId), cancellationToken);

        // 2. Create or Update UserPushToken in database
        var existingToken = await _context.UserPushTokens
            .Where(t => t.UserId == _currentUser.UserId && t.ExpoPushToken == request.ExpoPushToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingToken == null)
        {
            var newToken = UserPushToken.Create(
                _currentUser.UserId,
                request.ExpoPushToken,
                request.Platform,
                request.DeviceId
            );
            _context.UserPushTokens.Add(newToken);
        }
        else
        {
            existingToken.UpdateToken(request.ExpoPushToken, request.Platform, true); // Mark as active
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _mediator.Send(new SaveExpoPushTokenCommand(_currentUser.UserId.ToString()), cancellationToken);

        return Result.Success();
    }
}
