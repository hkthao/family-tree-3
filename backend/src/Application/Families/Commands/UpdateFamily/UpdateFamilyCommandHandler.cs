using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandler : IRequestHandler<UpdateFamilyCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateFamilyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
    {
        var filter = Builders<Family>.Filter.Eq(f => f.Id, request.Id);
        var entity = await _context.Families.Find(filter).FirstOrDefaultAsync(cancellationToken) 
                     ?? throw new NotFoundException(nameof(Family), request.Id);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Address = request.Address;

        await _context.Families.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }
}