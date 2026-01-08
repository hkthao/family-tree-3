using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Notifications.Commands.SaveExpoPushToken;

namespace backend.Application.UserPushTokens.Commands.DeleteUserPushToken;

public class DeleteUserPushTokenCommandHandler(IApplicationDbContext context, IMediator mediator) : IRequestHandler<DeleteUserPushTokenCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMediator _mediator = mediator; // Injected mediator

    public async Task<Result> Handle(DeleteUserPushTokenCommand request, CancellationToken cancellationToken)
    {
        // Check if the current user is authorized to delete this push token
        var entity = await _context.UserPushTokens
            .Where(t => t.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"UserPushToken with ID {request.Id}"), ErrorSources.NotFound);
        _context.UserPushTokens.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _mediator.Send(new SaveExpoPushTokenCommand(entity.UserId.ToString()), cancellationToken);

        return Result.Success();
    }
}
