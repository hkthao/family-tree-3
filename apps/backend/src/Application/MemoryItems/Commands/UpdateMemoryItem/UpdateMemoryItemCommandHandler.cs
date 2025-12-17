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
        var entity = await _context.MemoryItems
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
            .FirstOrDefaultAsync(mi => mi.Id == request.Id && mi.FamilyId == request.FamilyId, cancellationToken);

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
        var existingMediaIds = entity.MemoryMedia.Select(m => m.Id).ToHashSet();
        var requestMediaIds = request.Media.Where(m => m.Id.HasValue).Select(m => m.Id!.Value).ToHashSet();

        // Remove media not in request
        foreach (var media in entity.MemoryMedia.Where(m => !requestMediaIds.Contains(m.Id)).ToList())
        {
            _context.MemoryMedia.Remove(media);
        }

        // Add or update media
        foreach (var mediaDto in request.Media)
        {
            if (mediaDto.Id.HasValue)
            {
                var existingMedia = entity.MemoryMedia.FirstOrDefault(m => m.Id == mediaDto.Id);
                existingMedia?.Update(mediaDto.MediaType, mediaDto.Url);
            }
            else
            {
                entity.AddMedia(new MemoryMedia(entity.Id, mediaDto.MediaType, mediaDto.Url));
            }
        }

        // Handle Persons updates
        var existingPersonMemberIds = entity.MemoryPersons.Select(mp => mp.MemberId).ToHashSet();
        var requestPersonMemberIds = request.Persons.Select(mp => mp.MemberId).ToHashSet();

        // Remove persons not in request
        foreach (var person in entity.MemoryPersons.Where(mp => !requestPersonMemberIds.Contains(mp.MemberId)).ToList())
        {
            _context.MemoryPersons.Remove(person);
        }

        // Add persons not currently associated
        foreach (var personDto in request.Persons.Where(mp => !existingPersonMemberIds.Contains(mp.MemberId)))
        {
            // Check if member exists in the specified family
            var memberExists = await _context.Members.AnyAsync(m => m.Id == personDto.MemberId && m.FamilyId == request.FamilyId, cancellationToken);
            if (!memberExists)
            {
                return Result.Failure($"Member with ID {personDto.MemberId} not found in family {request.FamilyId}.");
            }
            _context.MemoryPersons.Add(new MemoryPerson(entity.Id, personDto.MemberId));
        }


        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
