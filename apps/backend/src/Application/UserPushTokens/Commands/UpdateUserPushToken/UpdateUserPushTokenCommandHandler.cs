using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Notifications.Commands.SaveExpoPushToken;
using backend.Application.Notifications.Commands.SyncSubscriber; // New using directive

namespace backend.Application.UserPushTokens.Commands.UpdateUserPushToken;

public class UpdateUserPushTokenCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IMediator mediator) : IRequestHandler<UpdateUserPushTokenCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IMediator _mediator = mediator; // Injected mediator

    public async Task<Result<Guid>> Handle(UpdateUserPushTokenCommand request, CancellationToken cancellationToken)
    {
        // Check if the current user is authorized to update this push token
        var entity = await _context.UserPushTokens
            .Where(t => t.Id == request.Id && t.UserId == _currentUser.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            // Return failure if not found or if the current user is not the owner
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"UserPushToken with ID {request.Id}"), ErrorSources.NotFound);
        }

        entity.UpdateToken(
            request.ExpoPushToken,
            request.Platform,
            request.IsActive
        );

        await _context.SaveChangesAsync(cancellationToken);

        // Synchronize subscriber with notification service
        await _mediator.Send(new SyncSubscriberCommand(_currentUser.UserId), cancellationToken);
        await _mediator.Send(new SaveExpoPushTokenCommand(entity.UserId.ToString()), cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}

