using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

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
        var filter = Builders<Relationship>.Filter.Eq(r => r.Id, request.Id!);
        var entity = await _context.Relationships.Find(filter).FirstOrDefaultAsync(cancellationToken)
                     ?? throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Relationship), request.Id!);

        entity.SourceMemberId = ObjectId.Parse(request.SourceMemberId!);
        entity.TargetMemberId = ObjectId.Parse(request.TargetMemberId!);
        entity.Type = request.Type;
        entity.FamilyId = ObjectId.Parse(request.FamilyId!);
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;

        await _context.Relationships.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }
}
