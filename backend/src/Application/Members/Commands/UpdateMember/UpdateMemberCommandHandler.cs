using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Members
            .Include(m => m.Relationships)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Member), request.Id);
        }

        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.Nickname = request.Nickname;
        entity.DateOfBirth = request.DateOfBirth;
        entity.DateOfDeath = request.DateOfDeath;
        entity.PlaceOfBirth = request.PlaceOfBirth;
        entity.PlaceOfDeath = request.PlaceOfDeath;
        entity.Gender = request.Gender;
        entity.AvatarUrl = request.AvatarUrl;
        entity.Occupation = request.Occupation;
        entity.Biography = request.Biography;
        entity.FamilyId = request.FamilyId;

        // Manage Relationships
        var existingRelationships = entity.Relationships.ToList();
        var incomingRelationships = request.Relationships.ToList();

        // Remove relationships explicitly marked for deletion
        foreach (var deletedRelId in request.DeletedRelationshipIds)
        {
            var relToRemove = existingRelationships.FirstOrDefault(er => er.Id == deletedRelId);
            if (relToRemove != null)
            {
                _context.Relationships.Remove(relToRemove);
            }
        }

        // Add or update relationships from the incoming list
        foreach (var incomingRel in incomingRelationships)
        {
            var existingRel = existingRelationships.FirstOrDefault(er => er.Id == incomingRel.Id);

            if (existingRel == null) // New relationship
            {
                _context.Relationships.Add(new Relationship
                {
                    SourceMemberId = entity.Id,
                    TargetMemberId = incomingRel.TargetMemberId,
                    Type = incomingRel.Type,
                    Order = incomingRel.Order
                });
            }
            else // Update existing relationship
            {
                existingRel.TargetMemberId = incomingRel.TargetMemberId;
                existingRel.Type = incomingRel.Type;
                existingRel.Order = incomingRel.Order;
            }
        }

        // Comment: Write-side invariant: Member and Relationships are updated in the database context.
        await _context.SaveChangesAsync(cancellationToken);
    }
}
