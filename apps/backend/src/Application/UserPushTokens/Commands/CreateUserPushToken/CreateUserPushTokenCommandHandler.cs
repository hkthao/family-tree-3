using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.UserPushTokens.Commands.CreateUserPushToken;

public class CreateUserPushTokenCommandHandler(IApplicationDbContext context, ICurrentUser currentUser) : IRequestHandler<CreateUserPushTokenCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<Guid>> Handle(CreateUserPushTokenCommand request, CancellationToken cancellationToken)
    {
        // Check if the current user is authorized to create a push token for the requested UserId
        if (_currentUser.UserId != request.UserId)
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

        return Result<Guid>.Success(entity.Id);
    }
}
