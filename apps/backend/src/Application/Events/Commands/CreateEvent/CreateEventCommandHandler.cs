using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services; // Add this for ILunarCalendarService
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;

namespace backend.Application.Events.Commands.CreateEvent;

public class CreateEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator, ILunarCalendarService lunarCalendarService) : IRequestHandler<CreateEventCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;
    private readonly ILunarCalendarService _lunarCalendarService = lunarCalendarService; // Injected service

    public async Task<Result<Guid>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        // Authorization check: Only family managers or admins can create events
        if (request.FamilyId.HasValue && !_authorizationService.CanManageFamily(request.FamilyId.Value))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        Event entity;

        // Determine which factory method to use based on CalendarType
        if (request.CalendarType == CalendarType.Solar)
        {
            if (!request.SolarDate.HasValue)
            {
                return Result<Guid>.Failure("Solar event must have a SolarDate.", ErrorSources.BadRequest);
            }
            if (request.LunarDate != null)
            {
                return Result<Guid>.Failure("Solar event cannot have a LunarDate.", ErrorSources.BadRequest);
            }
            entity = Event.CreateSolarEvent(
                request.Name,
                GenerateUniqueCode("EVT"),
                request.Type,
                request.SolarDate.Value,
                request.RepeatRule,
                request.FamilyId,
                request.Description,
                request.Color
            );
        }
        else if (request.CalendarType == CalendarType.Lunar)
        {
            if (request.LunarDate == null)
            {
                return Result<Guid>.Failure("Lunar event must have a LunarDate.", ErrorSources.BadRequest);
            }
            var lunarDateVO = new LunarDate(request.LunarDate.Day, request.LunarDate.Month, request.LunarDate.IsLeapMonth, request.LunarDate.IsEstimated);
            entity = Event.CreateLunarEvent(
                request.Name,
                GenerateUniqueCode("EVT"),
                request.Type,
                lunarDateVO,
                request.RepeatRule,
                request.FamilyId,
                request.Description,
                request.Color
            );
        }
        else
        {
            return Result<Guid>.Failure("Invalid CalendarType.", ErrorSources.BadRequest);
        }

        foreach (var memberId in request.EventMemberIds)
        {
            entity.AddEventMember(memberId);
        }

        _context.Events.Add(entity);

        // Handle LocationLink
        if (request.LocationId.HasValue)
        {
            var locationLink = LocationLink.Create(
                entity.Id.ToString(), // RefId is EventId
                RefType.Event,        // RefType is Event
                request.Location ?? string.Empty,         // Description from Location
                request.LocationId.Value,
                LocationLinkType.General // Specify LinkType for event
            );
            _context.LocationLinks.Add(locationLink);
        }

        // Generate EventOccurrences for the current year if it's a yearly repeating lunar event
        if (entity.CalendarType == CalendarType.Lunar && entity.RepeatRule == RepeatRule.Yearly && entity.LunarDate != null)
        {
            int currentYear = DateTime.Now.Year;

            // Check if an occurrence for this event and current year already exists
            bool occurrenceExists = await _context.EventOccurrences
                .AnyAsync(eo => eo.EventId == entity.Id && eo.Year == currentYear, cancellationToken);

            if (!occurrenceExists)
            {
                if (entity.LunarDate.Day.HasValue && entity.LunarDate.Month.HasValue)
                {
                    DateTime? solarOccurrenceDate = _lunarCalendarService.ConvertLunarToSolar(
                        entity.LunarDate.Day.Value,
                        entity.LunarDate.Month.Value,
                        currentYear,
                        entity.LunarDate.IsLeapMonth.GetValueOrDefault(false));

                    if (solarOccurrenceDate.HasValue)
                    {
                        var newOccurrence = EventOccurrence.Create(entity.Id, currentYear, solarOccurrenceDate.Value);
                        _context.EventOccurrences.Add(newOccurrence);
                    }
                }
            }
        }

        entity.AddDomainEvent(new Domain.Events.Events.EventCreatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }

    private string GenerateUniqueCode(string prefix)
    {
        // For simplicity, generate a GUID and take a substring.
        // In a real application, you'd want to ensure uniqueness against existing codes in the database.
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
