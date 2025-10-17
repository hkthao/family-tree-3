using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Commands.CreateEvents;

public class CreateEventsCommandHandler : IRequestHandler<CreateEventsCommand, Result<List<Guid>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;

    public CreateEventsCommandHandler(IApplicationDbContext context, IUser user, IAuthorizationService authorizationService)
    {
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
    }

    public async Task<Result<List<Guid>>> Handle(CreateEventsCommand request, CancellationToken cancellationToken)
    {
        var createdEventIds = new List<Guid>();
        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);

        foreach (var command in request.Events)
        {
            var family = await _context.Families
                .Include(f => f.FamilyUsers)
                .FirstOrDefaultAsync(f => f.Id == command.FamilyId, cancellationToken);

            if (family == null)
            {
                return Result<List<Guid>>.Failure($"Family with ID {command.FamilyId} not found.");
            }

            // Check authorization for the family
            if (!(_user.Roles != null && _user.Roles.Contains(SystemRole.Admin.ToString())) &&
                !family.FamilyUsers.Any(fu => fu.UserProfileId == currentUserProfile!.Id && fu.Role == FamilyRole.Manager))
            {
                return Result<List<Guid>>.Failure($"User is not authorized to create events in family {family.Name}.");
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
                    var member = await _context.Members.FindAsync(Guid.Parse(memberId));
                    if (member != null)
                    {
                        entity.RelatedMembers.Add(member);
                    }
                }
            }

            _context.Events.Add(entity);
            createdEventIds.Add(entity.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<Guid>>.Success(createdEventIds);
    }
}
