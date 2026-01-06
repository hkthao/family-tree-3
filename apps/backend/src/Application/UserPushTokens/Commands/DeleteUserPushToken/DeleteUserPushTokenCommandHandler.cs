using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.UserPushTokens.Commands.DeleteUserPushToken;

public class DeleteUserPushTokenCommandHandler(IApplicationDbContext context, ICurrentUser currentUser) : IRequestHandler<DeleteUserPushTokenCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result> Handle(DeleteUserPushTokenCommand request, CancellationToken cancellationToken)
    {
        // Check if the current user is authorized to delete this push token
        var entity = await _context.UserPushTokens
            .Where(t => t.Id == request.Id && t.UserId == _currentUser.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            // Return failure if not found or if the current user is not the owner
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"UserPushToken with ID {request.Id}"), ErrorSources.NotFound);
        }

        _context.UserPushTokens.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
