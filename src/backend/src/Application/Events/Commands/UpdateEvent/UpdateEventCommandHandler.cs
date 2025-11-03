using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<UpdateEventCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    public async Task<Result<bool>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        // Authorization check: Only family managers or admins can update events
        if (!_authorizationService.CanManageFamily(request.FamilyId!.Value))
        {
            return Result<bool>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entity = await _context.Events
            .Include(e => e.EventMembers)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<bool>.Failure(string.Format(ErrorMessages.EventNotFound, request.Id), ErrorSources.NotFound);
        }

        var relatedMembers = await _context.Members
            .Where(m => request.RelatedMembers.Contains(m.Id))
            .ToListAsync(cancellationToken);

        entity.UpdateEvent(
            request.Name,
            entity.Code, // Code is not updated via this command
            request.Description,
            request.StartDate,
            request.EndDate,
            request.Location,
            request.Type,
            request.Color
        );

        // Update related members
        // Remove members not in the new list
        foreach (var existingMember in entity.EventMembers.ToList())
        {
            if (!request.RelatedMembers.Contains(existingMember.MemberId))
            {
                entity.RemoveEventMember(existingMember.MemberId);
            }
        }

        // Add new members
        foreach (var memberId in request.RelatedMembers)
        {
            if (!entity.EventMembers.Any(em => em.MemberId == memberId))
            {
                entity.AddEventMember(memberId);
            }
        }

        entity.AddDomainEvent(new Domain.Events.Events.EventUpdatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
