using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;

namespace backend.Application.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator) : IRequestHandler<DeleteEventCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<bool>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(request.Id);
        if (entity == null)
            return Result<bool>.Failure($"Event with ID {request.Id} not found.", "NotFound");

        if (!_authorizationService.CanManageFamily(entity.FamilyId!.Value))
            return Result<bool>.Failure("Access denied. Only family managers or admins can delete events.", "Forbidden");

        var eventName = entity.Name; // Capture event name for activity summary

        _context.Events.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        // Record activity
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = _user.Id,
            ActionType = UserActionType.DeleteEvent,
            TargetType = TargetType.Event,
            TargetId = request.Id.ToString(),
            ActivitySummary = $"Deleted event '{eventName}'."
        }, cancellationToken);

        return Result<bool>.Success(true);
    }
}
