using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;
using backend.Application.Members.Specifications;
using backend.Domain.Entities;

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
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var membersSpec = new MembersByIdsSpec(request.RelatedMembers);
        var relatedMembers = await _context.Members
            .WithSpecification(membersSpec)
            .ToListAsync(cancellationToken);

        var entity = new Event(request.Name, request.Code ?? GenerateUniqueCode("EVT"), request.Type, request.FamilyId);
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

        foreach (var memberId in request.RelatedMembers)
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
