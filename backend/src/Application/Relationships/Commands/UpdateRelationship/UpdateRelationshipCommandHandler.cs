using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandHandler : IRequestHandler<UpdateRelationshipCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateRelationshipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateRelationshipCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Relationships.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Relationship), request.Id);
        }

        entity.SourceMemberId = request.SourceMemberId!;
        entity.TargetMemberId = request.TargetMemberId!;
        entity.Type = request.Type;
        entity.FamilyId = request.FamilyId!;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
