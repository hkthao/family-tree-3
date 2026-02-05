using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.MemoryItems.Commands.ImportMemoryItems;

public class ImportMemoryItemsCommandHandler : IRequestHandler<ImportMemoryItemsCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public ImportMemoryItemsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(ImportMemoryItemsCommand request, CancellationToken cancellationToken)
    {
        // Check if family exists
        var familyExists = await _context.Families.AnyAsync(f => f.Id == request.FamilyId, cancellationToken);
        if (!familyExists)
        {
            return Result<Unit>.Failure($"Family with ID {request.FamilyId} not found.");
        }

        var importedMemoryItems = new List<MemoryItem>();

        foreach (var itemDto in request.MemoryItems)
        {
            // Create a new MemoryItem entity using its constructor
            var memoryItem = new MemoryItem(
                request.FamilyId, // FamilyId is set via constructor
                itemDto.Title,
                itemDto.Description,
                itemDto.HappenedAt,
                itemDto.EmotionalTag
            );
            memoryItem.Id = Guid.NewGuid(); // Assign a new ID to avoid conflicts

            // Handle nested MemoryMedia
            if (itemDto.MemoryMedia != null && itemDto.MemoryMedia.Any())
            {
                foreach (var mediaDto in itemDto.MemoryMedia)
                {
                    memoryItem.AddMedia(new MemoryMedia(
                        memoryItem.Id,
                        mediaDto.Url
                    ));
                }
            }

            // Handle nested MemoryPersons
            if (itemDto.MemoryPersons != null && itemDto.MemoryPersons.Any())
            {
                // Validate if associated members exist in the target family
                var memberIds = itemDto.MemoryPersons.Select(mp => mp.MemberId).ToList();
                var existingMembers = await _context.Members
                    .Where(m => memberIds.Contains(m.Id) && m.FamilyId == request.FamilyId)
                    .Select(m => m.Id)
                    .ToListAsync(cancellationToken);

                var nonExistentMembers = memberIds.Except(existingMembers).ToList();
                if (nonExistentMembers.Any())
                {
                    return Result<Unit>.Failure($"One or more associated members not found in Family {request.FamilyId}: {string.Join(", ", nonExistentMembers)}");
                }

                foreach (var personDto in itemDto.MemoryPersons)
                {
                    memoryItem.AddPerson(new MemoryPerson(
                        memoryItem.Id,
                        personDto.MemberId
                    ));
                }
            }

            importedMemoryItems.Add(memoryItem);
        }

        _context.MemoryItems.AddRange(importedMemoryItems);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
