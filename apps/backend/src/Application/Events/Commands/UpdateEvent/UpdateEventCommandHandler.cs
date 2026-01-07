using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications; // Add this using statement
using backend.Domain.Entities; // Add this using statement
using backend.Domain.Enums; // Add this
using backend.Domain.ValueObjects; // Add this

namespace backend.Application.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator) : IRequestHandler<UpdateEventCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;
    public async Task<Result<bool>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        // Authorization check: Only family managers or admins can update events
        if (request.FamilyId.HasValue && !_authorizationService.CanManageFamily(request.FamilyId.Value))
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

        // Validation to ensure CalendarType is not changed during update
        // if (entity.CalendarType != request.CalendarType)
        // {
        //     return Result<bool>.Failure("CalendarType cannot be changed during event update.", ErrorSources.BadRequest);
        // }

        // Determine which update method to use based on CalendarType
        if (request.CalendarType == CalendarType.Solar)
        {
            if (!request.SolarDate.HasValue)
            {
                return Result<bool>.Failure("Solar event must have a SolarDate.", ErrorSources.BadRequest);
            }
            if (request.LunarDate != null)
            {
                return Result<bool>.Failure("Solar event cannot have a LunarDate.", ErrorSources.BadRequest);
            }
            entity.UpdateSolarEvent(
                request.Name,
                entity.Code, // Use existing code if not provided in request
                request.Description,
                request.SolarDate.Value,
                request.RepeatRule,
                request.Type,
                request.Color,
                request.Location // Pass Location here
            );
        }
        else if (request.CalendarType == CalendarType.Lunar)
        {
            if (request.LunarDate == null)
            {
                return Result<bool>.Failure("Lunar event must have a LunarDate.", ErrorSources.BadRequest);
            }
            if (request.SolarDate.HasValue)
            {
                return Result<bool>.Failure("Lunar event cannot have a SolarDate.", ErrorSources.BadRequest);
            }
            var lunarDateVO = new LunarDate(request.LunarDate.Day, request.LunarDate.Month, request.LunarDate.IsLeapMonth);
            entity.UpdateLunarEvent(
                request.Name,
                entity.Code,
                request.Description,
                lunarDateVO,
                request.RepeatRule,
                request.Type,
                request.Color,
                request.Location // Pass Location here
            );
        }
        else
        {
            return Result<bool>.Failure("Invalid CalendarType.", ErrorSources.BadRequest);
        }

        // Update related members
        // Remove existing EventMembers from the context
        _context.EventMembers.RemoveRange(entity.EventMembers);
        entity.ClearEventMembers(); // Clear the in-memory collection

        // Add new members
        foreach (var memberId in request.RelatedMemberIds)
        {
            entity.AddEventMember(memberId);
        }

        // Domain event is added within the UpdateSolarEvent/UpdateLunarEvent methods now.
        // entity.AddDomainEvent(new Domain.Events.Events.EventUpdatedEvent(entity));

        // Handle LocationLink
        var existingLocationLink = await _context.LocationLinks
            .FirstOrDefaultAsync(ll => ll.RefId == entity.Id.ToString() && ll.RefType == RefType.Event, cancellationToken);

        if (request.LocationId.HasValue)
        {
            if (existingLocationLink != null)
            {
                // Update existing LocationLink
                existingLocationLink.UpdateLocationDetails(request.LocationId.Value, request.Location ?? string.Empty);
            }
            else
            {
                // Create new LocationLink
                var newLocationLink = LocationLink.Create(
                    entity.Id.ToString(),
                    RefType.Event,
                    request.Location ?? string.Empty,
                    request.LocationId.Value,
                    LocationLinkType.General
                );
                _context.LocationLinks.Add(newLocationLink);
            }
        }
        else // request.LocationId has no value
        {
            if (existingLocationLink != null)
            {
                // Remove existing LocationLink
                _context.LocationLinks.Remove(existingLocationLink);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
