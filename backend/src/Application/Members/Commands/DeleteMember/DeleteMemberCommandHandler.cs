using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.Application.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var filter = Builders<Member>.Filter.Eq("_id", ObjectId.Parse(request.Id!));
        var result = await _context.Members.DeleteOneAsync(filter, cancellationToken: cancellationToken);

        if (result.DeletedCount == 0)
        {
            throw new NotFoundException(nameof(Member), request.Id!);
        }
    }
}
