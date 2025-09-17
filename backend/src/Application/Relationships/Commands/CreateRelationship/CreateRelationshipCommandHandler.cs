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
            SourceMemberId = ObjectId.Parse(request.MemberId!),
            Type = request.Type,
            TargetMemberId = ObjectId.Parse(request.TargetId!),
            FamilyId = ObjectId.Parse(request.FamilyId!),
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        await _context.Relationships.InsertOneAsync(entity, cancellationToken: cancellationToken);

        return entity.Id.ToString();
    }
}
