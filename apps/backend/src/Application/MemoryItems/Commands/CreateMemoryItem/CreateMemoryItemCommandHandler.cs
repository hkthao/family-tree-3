using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.MemoryItems.Commands.CreateMemoryItem;

public class CreateMemoryItemCommandHandler : IRequestHandler<CreateMemoryItemCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateMemoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateMemoryItemCommand request, CancellationToken cancellationToken)
    {
        // Check if family exists
        var familyExists = await _context.Families.AnyAsync(f => f.Id == request.FamilyId, cancellationToken);
        if (!familyExists)
        {
            return Result<Guid>.Failure("Family not found.");
        }

        var entity = new MemoryItem(
            request.FamilyId,
            request.Title,
            request.Description,
            request.HappenedAt,
            request.EmotionalTag
        );

        foreach (var mediaDto in request.MemoryMedia)
        {
            entity.AddMedia(new MemoryMedia(entity.Id, mediaDto.Url));
        }

        var deleteItems = await _context.MemoryMedia
            .Where(mm => request.DeletedMediaIds.Contains(mm.Id) && mm.MemoryItem.FamilyId == request.FamilyId)
            .ToListAsync(cancellationToken);
        if (deleteItems.Count != 0)
            _context.MemoryMedia.RemoveRange(deleteItems);

        foreach (var personId in request.PersonIds)
        {
            _context.MemoryPersons.Add(new MemoryPerson(entity.Id, personId));
        }

        _context.MemoryItems.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
