using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.Application.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandler : IRequestHandler<DeleteFamilyCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteFamilyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteFamilyCommand request, CancellationToken cancellationToken)
    {
        var filter = Builders<Family>.Filter.Eq("_id", ObjectId.Parse(request.Id));
        var result = await _context.Families.DeleteOneAsync(filter, cancellationToken: cancellationToken);

        if (result.DeletedCount == 0)
        {
            throw new NotFoundException(nameof(Family), request.Id);
        }
    }
}