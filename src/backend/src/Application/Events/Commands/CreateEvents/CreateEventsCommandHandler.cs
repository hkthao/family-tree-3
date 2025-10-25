using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Events;

namespace backend.Application.Events.Commands.CreateEvents;

public class CreateEventsCommandHandler(IApplicationDbContext context, IUser user, IAuthorizationService authorizationService) : IRequestHandler<CreateEventsCommand, Result<List<Guid>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<List<Guid>>> Handle(CreateEventsCommand request, CancellationToken cancellationToken)
    {
        var createdEventIds = new List<Guid>();

        foreach (var command in request.Events)
        {
            if (!command.FamilyId.HasValue)
            {
                return Result<List<Guid>>.Failure("FamilyId is required for event creation.");
            }

            // Check authorization for the family
            if (!_authorizationService.CanManageFamily(command.FamilyId.Value))
            {
                return Result<List<Guid>>.Failure($"User is not authorized to create events in family with ID {command.FamilyId.Value}.");
            }

            var entity = new Event
            {
                Name = command.Name,
                Type = command.Type,
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                Location = command.Location,
                Description = command.Description,
                FamilyId = command.FamilyId,
            };

            // Handle related members
            if (command.RelatedMembers != null && command.RelatedMembers.Any())
            {
                foreach (var memberId in command.RelatedMembers)
                {
                    entity.EventMembers.Add(new EventMember { EventId = entity.Id, MemberId = Guid.Parse(memberId) });
                }
            }

            entity.AddDomainEvent(new EventCreatedEvent(entity));
            _context.Events.Add(entity);
            createdEventIds.Add(entity.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<Guid>>.Success(createdEventIds);
    }
}
