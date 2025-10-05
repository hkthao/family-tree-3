using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added
using backend.Domain.Entities;

namespace backend.Application.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events
            .Include(e => e.RelatedMembers)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<bool>.Failure($"Event with ID {request.Id} not found.");
        }

        var relatedMembers = await _context.Members
            .Where(m => request.RelatedMembers.Contains(m.Id))
            .ToListAsync(cancellationToken);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Location = request.Location;
        entity.FamilyId = request.FamilyId;
        entity.Type = request.Type;
        entity.Color = request.Color;
        entity.RelatedMembers = relatedMembers; // Update related members

        // Comment: Write-side invariant: Event is updated in the database context.
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}