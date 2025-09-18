using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandHandler : IRequestHandler<CreateRelationshipCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateRelationshipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRelationshipCommand request, CancellationToken cancellationToken)
    {
        var entity = new Relationship
        {
            SourceMemberId = request.MemberId,
            Type = request.Type,
            TargetMemberId = request.TargetId,
            FamilyId = request.FamilyId!,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _context.Relationships.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
