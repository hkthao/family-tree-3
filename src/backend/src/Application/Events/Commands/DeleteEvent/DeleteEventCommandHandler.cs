using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IUser user, ILogger<DeleteEventCommandHandler> logger) : IRequestHandler<DeleteEventCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IUser _user = user;
    private readonly ILogger<DeleteEventCommandHandler> _logger = logger;

    public async Task<Result<bool>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(request.Id);
        if (entity == null)
            return Result<bool>.Failure($"Event with ID {request.Id} not found.", "NotFound");

        if (!_authorizationService.CanManageFamily(entity.FamilyId!.Value))
            return Result<bool>.Failure("Access denied. Only family managers or admins can delete events.", "Forbidden");

        entity.AddDomainEvent(new Domain.Events.Events.EventDeletedEvent(entity));

        _context.Events.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        // Record activity
        if (!_user.Id.HasValue)
        {
            _logger.LogWarning("Current user ID not found when recording activity for event deletion. Activity will not be recorded.");
            return Result<bool>.Failure("User is not authenticated.", "Authentication");
        }
        return Result<bool>.Success(true);
    }
}
