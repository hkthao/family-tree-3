using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

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
        var filter = Builders<Member>.Filter.Eq("_id", ObjectId.Parse(request.Id!));
        var update = Builders<Member>.Update
            .Set(m => m.FullName, request.FullName)
            .Set(m => m.DateOfBirth, request.DateOfBirth)
            .Set(m => m.DateOfDeath, request.DateOfDeath)
            .Set(m => m.Status, request.Status)
            .Set(m => m.Phone, request.Phone)
            .Set(m => m.Email, request.Email)
            .Set(m => m.Generation, request.Generation)
            .Set(m => m.DisplayOrder, request.DisplayOrder)
            .Set(m => m.FamilyId, ObjectId.Parse(request.FamilyId!))
            .Set(m => m.Description, request.Description);

        var result = await _context.Members.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new NotFoundException(nameof(Member), request.Id!);
        }
    }
}
