using MediatR;
using backend.Application.Services;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.Relationships.Queries;

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
