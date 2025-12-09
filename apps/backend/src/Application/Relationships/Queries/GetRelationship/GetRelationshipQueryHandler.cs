using System.Threading;
using System.Threading.Tasks;
using backend.Application.Relationships.Queries.GetRelationship; // Updated using directive
using backend.Application.Services;
using MediatR;

namespace backend.Application.Relationships.Queries.GetRelationship; // Updated namespace

public class GetRelationshipQueryHandler : IRequestHandler<GetRelationshipQuery, RelationshipDetectionResult>
{
    private readonly IRelationshipDetectionService _relationshipDetectionService;

    public GetRelationshipQueryHandler(IRelationshipDetectionService relationshipDetectionService)
    {
        _relationshipDetectionService = relationshipDetectionService;
    }

    public async Task<RelationshipDetectionResult> Handle(GetRelationshipQuery request, CancellationToken cancellationToken)
    {
        return await _relationshipDetectionService.DetectRelationshipAsync(request.FamilyId, request.MemberAId, request.MemberBId);
    }
}
