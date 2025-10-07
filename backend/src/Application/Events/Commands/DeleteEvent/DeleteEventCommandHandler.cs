using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using backend.Domain.Enums;
using backend.Application.UserActivities.Commands.RecordActivity;

namespace backend.Application.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediator _mediator;

    public DeleteEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mediator = mediator;
    }

    public async Task<Result<bool>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<bool>.Failure("User profile not found.", "NotFound");
        }

        var entity = await _context.Events.FindAsync(request.Id);

        if (entity == null)
        {
            return Result<bool>.Failure($"Event with ID {request.Id} not found.");
        }

        // Authorization check: Only family managers or admins can delete events
        if (!_authorizationService.IsAdmin() && (entity.FamilyId.HasValue && !_authorizationService.CanManageFamily(entity.FamilyId.Value, currentUserProfile)))
        {
            return Result<bool>.Failure("Access denied. Only family managers or admins can delete events.", "Forbidden");
        }

        var eventName = entity.Name; // Capture event name for activity summary

        _context.Events.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        // Record activity
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = currentUserProfile.Id,
            ActionType = UserActionType.DeleteEvent,
            TargetType = TargetType.Event,
            TargetId = request.Id.ToString(),
            ActivitySummary = $"Deleted event '{eventName}'."
        }, cancellationToken);

        return Result<bool>.Success(true);
    }
}
