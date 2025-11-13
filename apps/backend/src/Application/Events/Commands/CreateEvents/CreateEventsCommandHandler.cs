using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Events.Events;

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
            if (!command.FamilyId.HasValue)
            {
                return Result<List<Guid>>.Failure(string.Format(ErrorMessages.NotFound, command.FamilyId), ErrorSources.NotFound);
            }

            // Check authorization for the family
            if (!_authorizationService.CanManageFamily(command.FamilyId.Value))
            {
                return Result<List<Guid>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }

            var entity = new Event(command.Name, "", command.Type, command.FamilyId); // Code is empty for now, will be generated or set later
            entity.UpdateEvent(
                command.Name,
                entity.Code, // Code is not updated via this command
                command.Description,
                command.StartDate,
                command.EndDate,
                command.Location,
                command.Type,
                entity.Color // Color is not updated via this command
            );

            // Handle related members
            if (command.RelatedMembers != null && command.RelatedMembers.Any())
            {
                foreach (var memberId in command.RelatedMembers)
                {
                    entity.AddEventMember(Guid.Parse(memberId));
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
