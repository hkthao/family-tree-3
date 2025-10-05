using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Mappings;
using backend.Application.Relationships.Specifications;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Queries.SearchRelationships;

public class SearchRelationshipsQueryHandler : IRequestHandler<SearchRelationshipsQuery, Result<PaginatedList<RelationshipListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchRelationshipsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<RelationshipListDto>>> Handle(SearchRelationshipsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Relationships.AsQueryable();

        // Apply individual specifications
        query = query.WithSpecification(new RelationshipBySourceMemberIdSpecification(request.SourceMemberId));
        query = query.WithSpecification(new RelationshipByTargetMemberIdSpecification(request.TargetMemberId));
        query = query.WithSpecification(new RelationshipByTypeSpecification(request.Type));

        // Include related members for full name display
        query = query.Include(r => r.SourceMember).Include(r => r.TargetMember);

        // Apply ordering specification
        query = query.WithSpecification(new RelationshipOrderingSpecification(request.SortBy, request.SortOrder));

        var paginatedList = await query
            .ProjectTo<RelationshipListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<RelationshipListDto>>.Success(paginatedList);
    }
}
