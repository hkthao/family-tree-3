using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.Commands.CreateEvent;

public class CreateEventCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<CreateEventCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<Guid>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        // Authorization check: Only family managers or admins can create events
        if (!_authorizationService.CanManageFamily(request.FamilyId!.Value))
        {
            return Result<Guid>.Failure("Access denied. Only family managers or admins can create events.", "Forbidden");
        }

        var relatedMembers = await _context.Members
            .Where(m => request.RelatedMembers.Contains(m.Id))
            .ToListAsync(cancellationToken);

        var entity = new Event
        {
            Name = request.Name,
            Code = request.Code ?? GenerateUniqueCode("EVT"),
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            FamilyId = request.FamilyId,
            Type = request.Type,
            Color = request.Color,
            EventMembers = relatedMembers.Select(m => new EventMember { MemberId = m.Id }).ToList()
        };

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
