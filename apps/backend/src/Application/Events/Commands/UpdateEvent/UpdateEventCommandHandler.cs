using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;
using backend.Application.Members.Specifications;

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

        var eventSpec = new EventByIdSpecification(request.Id, true);
        var entity = await _context.Events
            .WithSpecification(eventSpec)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<bool>.Failure(string.Format(ErrorMessages.EventNotFound, request.Id), ErrorSources.NotFound);
        }

        var membersSpec = new MembersByIdsSpec(request.RelatedMemberIds);
        var relatedMembers = await _context.Members
            .WithSpecification(membersSpec)
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
        // Remove existing EventMembers from the context
        _context.EventMembers.RemoveRange(entity.EventMembers);
        entity.ClearEventMembers(); // Clear the in-memory collection

        // Add new members
        foreach (var memberId in request.RelatedMemberIds)
        {
            entity.AddEventMember(memberId);
        }

        entity.AddDomainEvent(new Domain.Events.Events.EventUpdatedEvent(entity)); // NEW

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
