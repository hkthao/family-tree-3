using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
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
        var filter = Builders<Family>.Filter.Eq("_id", ObjectId.Parse(request.Id));
        var update = Builders<Family>.Update
            .Set(f => f.Name, request.Name)
            .Set(f => f.Address, request.Address)
            .Set(f => f.Logo, request.Logo)
            .Set(f => f.History, request.History);

        var result = await _context.Families.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new NotFoundException(nameof(Family), request.Id!);
        }
    }
}