using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandHandler : IRequestHandler<CreateRelationshipCommand, string>
{
    private readonly IApplicationDbContext _context;

    public CreateRelationshipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(CreateRelationshipCommand request, CancellationToken cancellationToken)
    {
        var entity = new Relationship
        {
            MemberId = ObjectId.Parse(request.MemberId!),
            Type = request.Type,
            TargetId = ObjectId.Parse(request.TargetId!)
        };

        await _context.Relationships.InsertOneAsync(entity, cancellationToken: cancellationToken);

        return entity.Id.ToString();
    }
}
