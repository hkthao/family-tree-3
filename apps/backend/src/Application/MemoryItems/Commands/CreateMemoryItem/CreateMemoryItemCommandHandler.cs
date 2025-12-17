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

        foreach (var mediaDto in request.Media)
        {
            entity.AddMedia(new MemoryMedia(entity.Id, mediaDto.MediaType, mediaDto.Url));
        }

        foreach (var personDto in request.Persons)
        {
            // Check if member exists in the specified family
            var memberExists = await _context.Members.AnyAsync(m => m.Id == personDto.MemberId && m.FamilyId == request.FamilyId, cancellationToken);
            if (!memberExists)
            {
                return Result<Guid>.Failure($"Member with ID {personDto.MemberId} not found in family {request.FamilyId}.");
            }
            // MemoryPerson does not have an Id property of its own, it's a join entity
            _context.MemoryPersons.Add(new MemoryPerson(entity.Id, personDto.MemberId));
        }

        _context.MemoryItems.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
