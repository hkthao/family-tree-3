using backend.Application.Common.Interfaces;

namespace backend.Application.Relationships.Queries.DetectRelationship; // Updated namespace

public class DetectRelationshipQueryHandler : IRequestHandler<DetectRelationshipQuery, RelationshipDetectionResult>
{
    private readonly IRelationshipDetectionService _relationshipDetectionService;

    public DetectRelationshipQueryHandler(IRelationshipDetectionService relationshipDetectionService)
    {
        _relationshipDetectionService = relationshipDetectionService;
    }

    public async Task<RelationshipDetectionResult> Handle(DetectRelationshipQuery request, CancellationToken cancellationToken)
    {
        return await _relationshipDetectionService.DetectRelationshipAsync(request.FamilyId, request.MemberAId, request.MemberBId, cancellationToken);
    }
}
