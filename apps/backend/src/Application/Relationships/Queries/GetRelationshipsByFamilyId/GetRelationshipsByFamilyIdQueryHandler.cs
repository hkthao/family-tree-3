using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;

namespace backend.Application.Relationships.Queries.GetRelationshipsByFamilyId;

public class GetRelationshipsByFamilyIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetRelationshipsByFamilyIdQuery, Result<List<RelationshipDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<List<RelationshipDto>>> Handle(GetRelationshipsByFamilyIdQuery request, CancellationToken cancellationToken)
    {
        var relationships = await _context.Relationships
            .Where(r => r.SourceMember.FamilyId == request.FamilyId || r.TargetMember.FamilyId == request.FamilyId)
            .ProjectTo<RelationshipDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        if (!relationships.Any())
        {
            return Result<List<RelationshipDto>>.Success(new List<RelationshipDto>()); // Return an empty list if no relationships found
        }

        return Result<List<RelationshipDto>>.Success(relationships);
    }
}
