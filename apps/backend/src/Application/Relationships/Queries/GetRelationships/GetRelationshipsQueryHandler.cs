using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Relationships.Specifications;

namespace backend.Application.Relationships.Queries.GetRelationships;

public class GetRelationshipsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser) : IRequestHandler<GetRelationshipsQuery, Result<PaginatedList<RelationshipListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<PaginatedList<RelationshipListDto>>> Handle(GetRelationshipsQuery request, CancellationToken cancellationToken)
    {
        // Handle unauthenticated user first, return empty list
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
        {
            return Result<PaginatedList<RelationshipListDto>>.Success(PaginatedList<RelationshipListDto>.Empty());
        }

        var query = _context.Relationships.AsQueryable();

        // Apply RelationshipAccessSpecification to filter relationships based on user's access
        query = query.WithSpecification(new RelationshipAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));

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
