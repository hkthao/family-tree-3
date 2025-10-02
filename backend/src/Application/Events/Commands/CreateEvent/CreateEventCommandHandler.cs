using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Events.Commands.CreateEvent;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = new Event
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            FamilyId = request.FamilyId,
            Type = request.Type,
            Color = request.Color,
        };

        if (request.RelatedMembers.Any())
        {
            var members = await _context.Members
                .Where(m => request.RelatedMembers.Contains(m.Id))
                .ToListAsync(cancellationToken);
            entity.RelatedMembers = members;
        }

        _context.Events.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
