using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Relationships.Specifications;

namespace backend.Application.Relationships.Queries.GetRelationshipById;

public class GetRelationshipByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetRelationshipByIdQuery, Result<RelationshipDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<RelationshipDto>> Handle(GetRelationshipByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Relationships.AsQueryable();
        query = query.WithSpecification(new RelationshipByIdSpecification(request.Id));
        query = query.WithSpecification(new RelationshipIncludeSpecifications());
        var relationshipDto = await query
            .ProjectTo<RelationshipDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        return relationshipDto == null
            ? Result<RelationshipDto>.Failure($"Relationship with ID {request.Id} not found.")
            : Result<RelationshipDto>.Success(relationshipDto);
    }
}
