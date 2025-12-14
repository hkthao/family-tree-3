using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Relationships.Specifications;

namespace backend.Application.Relationships.Queries.GetRelationships;

public class GetRelationshipsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser) : IRequestHandler<GetRelationshipsQuery, Result<IList<RelationshipListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<IList<RelationshipListDto>>> Handle(GetRelationshipsQuery request, CancellationToken cancellationToken)
    {
        // Handle unauthenticated user first, return empty list
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            return Result<IList<RelationshipListDto>>.Success([]);

        var query = _context.Relationships.AsQueryable();

        // Apply RelationshipAccessSpecification to filter relationships based on user's access
        query = query.WithSpecification(new RelationshipAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));
        query = query.WithSpecification(new RelationshipByFamilyIdSpecification(request.FamilyId));
        query = query.WithSpecification(new RelationshipIncludeSpecifications());

        var results = await query
            .ProjectTo<RelationshipListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IList<RelationshipListDto>>.Success(results);
    }
}
