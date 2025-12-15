using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums; // Add this
using backend.Domain.ValueObjects; // Add this

namespace backend.Application.Events.Commands.CreateEvent;

public class CreateEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<CreateEventCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
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
                request.Code ?? GenerateUniqueCode("EVT"),
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
            if (request.SolarDate.HasValue)
            {
                return Result<Guid>.Failure("Lunar event cannot have a SolarDate.", ErrorSources.BadRequest);
            }
            var lunarDateVO = new LunarDate(request.LunarDate.Day, request.LunarDate.Month, request.LunarDate.IsLeapMonth);
            entity = Event.CreateLunarEvent(
                request.Name,
                request.Code ?? GenerateUniqueCode("EVT"),
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

        foreach (var memberId in request.RelatedMemberIds)
        {
            entity.AddEventMember(memberId);
        }

        _context.Events.Add(entity);

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
