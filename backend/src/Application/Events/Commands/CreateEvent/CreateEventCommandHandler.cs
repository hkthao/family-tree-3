using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Commands.CreateEvent;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediator _mediator;

    public CreateEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<Guid>.Failure("User profile not found.", "NotFound");
        }

        // Authorization check: Only family managers or admins can create events
        if (!_authorizationService.IsAdmin() && (request.FamilyId.HasValue && !_authorizationService.CanManageFamily(request.FamilyId.Value, currentUserProfile)))
        {
            return Result<Guid>.Failure("Access denied. Only family managers or admins can create events.", "Forbidden");
        }

        var relatedMembers = await _context.Members
            .Where(m => request.RelatedMembers.Contains(m.Id))
            .ToListAsync(cancellationToken);

        var entity = new Event
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            FamilyId = request.FamilyId,
            Type = request.Type,
            Color = request.Color,
            RelatedMembers = relatedMembers
        };

        _context.Events.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        // Record activity
        await _mediator.Send(new RecordActivityCommand
        {
            UserProfileId = currentUserProfile.Id,
            ActionType = UserActionType.CreateEvent,
            TargetType = TargetType.Event,
            TargetId = entity.Id.ToString(),
            ActivitySummary = $"Created event '{entity.Name}'."
        }, cancellationToken); return Result<Guid>.Success(entity.Id);
    }
}
