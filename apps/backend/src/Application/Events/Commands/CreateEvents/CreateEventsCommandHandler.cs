using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums; // Add this
using backend.Domain.ValueObjects; // Add this

namespace backend.Application.Events.Commands.CreateEvents;

public class CreateEventsCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<CreateEventsCommand, Result<List<Guid>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<List<Guid>>> Handle(CreateEventsCommand request, CancellationToken cancellationToken)
    {
        var createdEventIds = new List<Guid>();

        foreach (var command in request.Events)
        {
            // Perform basic validation before authorization
            if (!command.FamilyId.HasValue || command.FamilyId.Value == Guid.Empty)
            {
                return Result<List<Guid>>.Failure(ErrorMessages.FamilyIdRequired, ErrorSources.BadRequest);
            }

            // Check authorization for the family
            if (!_authorizationService.CanManageFamily(command.FamilyId.Value))
            {
                return Result<List<Guid>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }

            Event entity;
            string eventCode = command.Code ?? GenerateUniqueCode("EVT");

            // Determine which factory method to use based on CalendarType
            if (command.CalendarType == CalendarType.Solar)
            {
                if (!command.SolarDate.HasValue)
                {
                    return Result<List<Guid>>.Failure("Solar event must have a SolarDate.", ErrorSources.BadRequest);
                }
                if (command.LunarDate != null)
                {
                    return Result<List<Guid>>.Failure("Solar event cannot have a LunarDate.", ErrorSources.BadRequest);
                }
                entity = Event.CreateSolarEvent(
                    command.Name,
                    eventCode,
                    command.Type,
                    command.SolarDate.Value,
                    command.RepeatRule,
                    command.FamilyId,
                    command.Description,
                    command.Color
                );
            }
            else if (command.CalendarType == CalendarType.Lunar)
            {
                if (command.LunarDate == null)
                {
                    return Result<List<Guid>>.Failure("Lunar event must have a LunarDate.", ErrorSources.BadRequest);
                }
                if (command.SolarDate.HasValue)
                {
                    return Result<List<Guid>>.Failure("Lunar event cannot have a SolarDate.", ErrorSources.BadRequest);
                }
                var lunarDateVO = new LunarDate(command.LunarDate.Day, command.LunarDate.Month, command.LunarDate.IsLeapMonth, command.LunarDate.IsEstimated);
                entity = Event.CreateLunarEvent(
                    command.Name,
                    eventCode,
                    command.Type,
                    lunarDateVO,
                    command.RepeatRule,
                    command.FamilyId,
                    command.Description,
                    command.Color
                );
            }
            else
            {
                return Result<List<Guid>>.Failure("Invalid CalendarType.", ErrorSources.BadRequest);
            }

            // Handle related members
            if (command.RelatedMembers != null && command.RelatedMembers.Any())
            {
                foreach (var memberIdString in command.RelatedMembers)
                {
                    if (Guid.TryParse(memberIdString, out Guid memberId))
                    {
                        entity.AddEventMember(memberId);
                    }
                    else
                    {
                        // Log or handle invalid member ID string if necessary
                        // For now, we will skip invalid IDs
                    }
                }
            }

            // Domain event is added within the factory methods now.
            _context.Events.Add(entity);
            createdEventIds.Add(entity.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<Guid>>.Success(createdEventIds);
    }

    private string GenerateUniqueCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
