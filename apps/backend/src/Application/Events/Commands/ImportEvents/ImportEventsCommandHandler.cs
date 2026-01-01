using backend.Application.Common.Constants; // Added
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums; // Added
using backend.Domain.ValueObjects; // Added for LunarDate



namespace backend.Application.Events.Commands.ImportEvents;

public class ImportEventsCommandHandler : IRequestHandler<ImportEventsCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService; // Added

    public ImportEventsCommandHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService) // Added authorizationService
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService; // Assign it
    }

    public async Task<Result<Unit>> Handle(ImportEventsCommand request, CancellationToken cancellationToken)
    {
        // Check if family exists
        var familyExists = await _context.Families.AnyAsync(f => f.Id == request.FamilyId, cancellationToken);
        if (!familyExists)
        {
            return Result<Unit>.Failure($"Family with ID {request.FamilyId} not found.");
        }

        // Authorization check: Only family managers or admins can import events
        if (!_authorizationService.CanManageFamily(request.FamilyId))
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var importedEvents = new List<Event>();

        foreach (var eventDto in request.Events)
        {
            Event eventEntity;

            if (eventDto.CalendarType == CalendarType.Solar)
            {
                if (!eventDto.SolarDate.HasValue)
                {
                    return Result<Unit>.Failure($"Solar event '{eventDto.Name}' must have a SolarDate.");
                }
                eventEntity = Event.CreateSolarEvent(
                    eventDto.Name,
                    eventDto.Code ?? Guid.NewGuid().ToString(), // Generate code if not provided
                    eventDto.Type,
                    eventDto.SolarDate.Value,
                    eventDto.RepeatRule,
                    request.FamilyId,
                    eventDto.Description,
                    eventDto.Color
                );
            }
            else // Lunar event
            {
                if (eventDto.LunarDate == null)
                {
                    return Result<Unit>.Failure($"Lunar event '{eventDto.Name}' must have LunarDate details.");
                }
                var lunarDate = new LunarDate(
                    eventDto.LunarDate.Day,
                    eventDto.LunarDate.Month,
                    eventDto.LunarDate.IsLeapMonth
                );
                eventEntity = Event.CreateLunarEvent(
                    eventDto.Name,
                    eventDto.Code ?? Guid.NewGuid().ToString(), // Generate code if not provided
                    eventDto.Type,
                    lunarDate,
                    eventDto.RepeatRule,
                    request.FamilyId,
                    eventDto.Description,
                    eventDto.Color
                );
            }
            eventEntity.Id = Guid.NewGuid(); // Assign a new ID

            // Handle EventMembers
            if (eventDto.RelatedMembers != null && eventDto.RelatedMembers.Any())
            {
                var memberIds = eventDto.RelatedMembers.Select(rm => rm.Id).ToList();
                var existingMembers = await _context.Members
                    .Where(m => memberIds.Contains(m.Id) && m.FamilyId == request.FamilyId)
                    .Select(m => m.Id)
                    .ToListAsync(cancellationToken);

                var nonExistentMembers = memberIds.Except(existingMembers).ToList();
                if (nonExistentMembers.Any())
                {
                    return Result<Unit>.Failure($"One or more associated members not found in Family {request.FamilyId}: {string.Join(", ", nonExistentMembers)}");
                }

                foreach (var memberId in existingMembers)
                {
                    eventEntity.AddEventMember(memberId);
                }
            }

            importedEvents.Add(eventEntity);
        }

        _context.Events.AddRange(importedEvents);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
