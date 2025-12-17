using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
namespace backend.Application.MemoryItems.Commands.UpdateMemoryItem;

public class UpdateMemoryItemCommandHandler : IRequestHandler<UpdateMemoryItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateMemoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result> Handle(UpdateMemoryItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MemoryItems.FirstOrDefaultAsync(mi => mi.Id == request.Id && mi.FamilyId == request.FamilyId, cancellationToken);
        if (entity == null)
        {
            return Result.NotFound();
        }
        entity.Update(
            request.Title,
            request.Description,
            request.HappenedAt,
            request.EmotionalTag
        );
        // Handle Media updates
        var deleteItems = await _context.MemoryMedia
            .Where(mm => request.DeletedMediaIds.Contains(mm.Id) && mm.MemoryItem.FamilyId == request.FamilyId)
            .ToListAsync(cancellationToken);
        if (deleteItems.Count != 0)
            _context.MemoryMedia.RemoveRange(deleteItems);
        // Add or update media
        foreach (var mediaDto in request.Media)
        {
            var existingMedia = entity.MemoryMedia.FirstOrDefault(m => m.Id == mediaDto.Id);
            if (existingMedia == null)
            {
                _context.MemoryMedia.Add(new MemoryMedia(entity.Id, mediaDto.Url));
            }
            else
            {
                existingMedia.Update(mediaDto.Url);
            }
        }
        // Handle MemoryPersons updates
        var existingMemoryPersons = await _context.MemoryPersons
            .Where(mp => mp.MemoryItemId == entity.Id)
            .ToListAsync(cancellationToken);

        var existingPersonIds = existingMemoryPersons.Select(mp => mp.MemberId).ToHashSet();

        // Remove MemoryPersons no longer in the request
        var personsToRemove = existingMemoryPersons
            .Where(mp => !request.PersonIds.Contains(mp.MemberId))
            .ToList();
        _context.MemoryPersons.RemoveRange(personsToRemove);

        // Add new MemoryPersons
        var personsToAddIds = request.PersonIds
            .Where(personId => !existingPersonIds.Contains(personId))
            .ToList();

        foreach (var personId in personsToAddIds)
        {
            _context.MemoryPersons.Add(new MemoryPerson(entity.Id, personId));
        }
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
