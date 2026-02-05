using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Notifications.Commands.SaveExpoPushToken;
using backend.Application.Notifications.Commands.SyncSubscriber; // New using directive
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.UserPushTokens.Commands.SyncCurrentUserPushToken;

public class SyncCurrentUserPushTokenCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IMediator mediator, ILogger<SyncCurrentUserPushTokenCommandHandler> logger) : IRequestHandler<SyncCurrentUserPushTokenCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<SyncCurrentUserPushTokenCommandHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(SyncCurrentUserPushTokenCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<Guid>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        // Check if a UserPushToken with the same DeviceId and UserId already exists and is active
        var existingToken = await _context.UserPushTokens
            .Where(t => t.UserId == _currentUser.UserId && t.DeviceId == request.DeviceId && t.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingToken != null)
        {
            // If an active token for this device and user exists, update it instead of creating a new one
            existingToken.UpdateToken(request.ExpoPushToken, request.Platform, true); // Ensure it's active
            _context.UserPushTokens.Update(existingToken);
            await _context.SaveChangesAsync(cancellationToken);
            await _mediator.Send(new SyncSubscriberCommand(_currentUser.UserId), cancellationToken);
            await _mediator.Send(new SaveExpoPushTokenCommand(_currentUser.UserId.ToString()), cancellationToken);
            return Result<Guid>.Success(existingToken.Id);
        }

        var entity = UserPushToken.Create(
            _currentUser.UserId,
            request.ExpoPushToken,
            request.Platform,
            request.DeviceId
        );

        _context.UserPushTokens.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _mediator.Send(new SyncSubscriberCommand(_currentUser.UserId), cancellationToken);
        await _mediator.Send(new SaveExpoPushTokenCommand(_currentUser.UserId.ToString()), cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
