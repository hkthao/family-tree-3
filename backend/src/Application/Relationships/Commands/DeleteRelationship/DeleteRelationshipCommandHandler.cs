using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.Application.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandler : IRequestHandler<DeleteRelationshipCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRelationshipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteRelationshipCommand request, CancellationToken cancellationToken)
    {
        var filter = Builders<Relationship>.Filter.Eq("_id", ObjectId.Parse(request.Id!));
        var result = await _context.Relationships.DeleteOneAsync(filter, cancellationToken: cancellationToken);

        if (result.DeletedCount == 0)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Relationship), request.Id!);
        }
    }
}
