using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser user, IAuthorizationService authorizationService) : IRequestHandler<GetFamiliesQuery, Result<IReadOnlyList<FamilyListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _user = user;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<IReadOnlyList<FamilyListDto>>> Handle(GetFamiliesQuery request, CancellationToken cancellationToken)
    {

        var query = _context.Families.AsQueryable();
        // If the user has the 'Admin' role, bypass family-specific access checks
        if (_authorizationService.IsAdmin())
        {
            // Admin has access to all families, no further filtering by user profile needed
        }
        else
        {
            // For non-admin users, apply family-specific access checks
            // Apply user access specification
            query = query.WithSpecification(new FamilyByUserIdSpec(_user.UserId));

        }
        // Apply other specifications
        query = query.WithSpecification(new FamilySearchTermSpecification(request.SearchTerm));
        query = query.WithSpecification(new FamilyOrderingSpecification(request.SortBy, request.SortOrder));
        query = query.WithSpecification(new FamilyPaginationSpecification((request.Page - 1) * request.ItemsPerPage, request.ItemsPerPage));

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var familyList = await query
            .ProjectTo<FamilyListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<FamilyListDto>>.Success(familyList);
    }
}
