using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Relationships.Specifications;

namespace backend.Application.Relationships.Queries.GetRelationships;

public class GetRelationshipsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetRelationshipsQuery, Result<PaginatedList<RelationshipListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<RelationshipListDto>>> Handle(GetRelationshipsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Relationships.AsQueryable();

        // Apply individual specifications
        query = query.WithSpecification(new RelationshipByFamilyIdSpecification(request.FamilyId));
        query = query.WithSpecification(new RelationshipBySourceMemberIdSpecification(request.SourceMemberId));
        query = query.WithSpecification(new RelationshipByTargetMemberIdSpecification(request.TargetMemberId));
        query = query.WithSpecification(new RelationshipByTypeSpecification(request.Type));
        query = query.WithSpecification(new RelationshipIncludeSpecifications());

        // Apply ordering specification
        query = query.WithSpecification(new RelationshipOrderingSpecification(request.SortBy, request.SortOrder));

        var paginatedList = await query
            .ProjectTo<RelationshipListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<RelationshipListDto>>.Success(paginatedList);
    }
}
