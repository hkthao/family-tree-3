using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator) : IRequestHandler<UpdateEventCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<bool>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        // Authorization check: Only family managers or admins can update events
        if (!_authorizationService.CanManageFamily(request.FamilyId!.Value))
        {
            return Result<bool>.Failure("Access denied. Only family managers or admins can update events.", "Forbidden");
        }

        var entity = await _context.Events
            .Include(e => e.EventMembers)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<bool>.Failure($"Event with ID {request.Id} not found.", "NotFound");
        }

        var oldName = entity.Name; // Capture old name for activity summary

        var relatedMembers = await _context.Members
            .Where(m => request.RelatedMembers.Contains(m.Id))
            .ToListAsync(cancellationToken);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Location = request.Location;
        entity.FamilyId = request.FamilyId;
        entity.Type = request.Type;
        entity.Color = request.Color;

        // Update related members
        entity.EventMembers.Clear();
        foreach (var member in relatedMembers)
        {
            entity.EventMembers.Add(new EventMember { EventId = entity.Id, MemberId = member.Id });
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Record activity
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = currentUserProfile.Id,
            ActionType = UserActionType.UpdateEvent,
            TargetType = TargetType.Event,
            TargetId = entity.Id.ToString(),
            ActivitySummary = $"Updated event '{oldName}' to '{entity.Name}'."
        }, cancellationToken);

        return Result<bool>.Success(true);
    }
}
