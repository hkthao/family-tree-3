using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events
            .Include(e => e.RelatedMembers)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Event), request.Id);
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Location = request.Location;
        entity.FamilyId = request.FamilyId;
        entity.Type = request.Type;
        entity.Color = request.Color;

        if (request.RelatedMembers.Any())
        {
            var members = await _context.Members
                .Where(m => request.RelatedMembers.Contains(m.Id))
                .ToListAsync(cancellationToken);
            entity.RelatedMembers = members;
        }
        else
        {
            entity.RelatedMembers.Clear();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
