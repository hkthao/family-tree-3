using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
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
        var result = await _context.Families.DeleteOneAsync(Builders<Family>.Filter.Eq(f => f.Id, request.Id), cancellationToken);

        if (result.DeletedCount == 0)
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Family), request.Id);
    }
}