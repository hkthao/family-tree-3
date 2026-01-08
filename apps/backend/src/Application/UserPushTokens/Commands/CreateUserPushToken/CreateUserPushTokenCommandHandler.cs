using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Notifications.Commands.SaveExpoPushToken; // New using directive
using backend.Application.Notifications.Commands.SyncSubscriber; // New using directive
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.UserPushTokens.Commands.CreateUserPushToken;

public class CreateUserPushTokenCommandHandler(IApplicationDbContext context, ICurrentUser currentUserService, IMediator mediator, ILogger<CreateUserPushTokenCommandHandler> logger) : IRequestHandler<CreateUserPushTokenCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUserService = currentUserService;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<CreateUserPushTokenCommandHandler> _logger = logger; // Injected logger

    public async Task<Result<Guid>> Handle(CreateUserPushTokenCommand request, CancellationToken cancellationToken)
    {
        // Check if the current user is authorized to create a push token for the requested UserId
        if (_currentUserService.UserId != request.UserId)
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Check if a UserPushToken with the same DeviceId and UserId already exists and is active
        var existingToken = await _context.UserPushTokens
            .Where(t => t.UserId == request.UserId && t.DeviceId == request.DeviceId && t.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingToken != null)
        {
            // If an active token for this device and user exists, update it instead of creating a new one
            existingToken.UpdateToken(request.ExpoPushToken, request.Platform, true); // Ensure it's active
            _context.UserPushTokens.Update(existingToken);
            await _context.SaveChangesAsync(cancellationToken);
            await _mediator.Send(new SyncSubscriberCommand(request.UserId), cancellationToken);
            await _mediator.Send(new SaveExpoPushTokenCommand(request.UserId.ToString()), cancellationToken);
            return Result<Guid>.Success(existingToken.Id);
        }

        var entity = UserPushToken.Create(
            request.UserId,
            request.ExpoPushToken,
            request.Platform,
            request.DeviceId
        );

        _context.UserPushTokens.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _mediator.Send(new SyncSubscriberCommand(request.UserId), cancellationToken);
        await _mediator.Send(new SaveExpoPushTokenCommand(request.UserId.ToString()), cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }


}
