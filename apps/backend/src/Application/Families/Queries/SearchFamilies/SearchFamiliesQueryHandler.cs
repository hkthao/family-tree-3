using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser, IAuthorizationService authorizationService, IPrivacyService privacyService) : IRequestHandler<SearchFamiliesQuery, Result<PaginatedList<FamilyDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<PaginatedList<FamilyDto>>> Handle(SearchFamiliesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Families.AsQueryable();

        // Apply family following filter
        if (request.IsFollowing.HasValue)
        {
            var userId = _currentUser.UserId;
            if (!_currentUser.IsAuthenticated)
            {
                // If IsFollowing is requested but user is not authenticated, no families are followed/unfollowed by this user
                // If filtering for followed families (true), return empty list.
                // If filtering for unfollowed families (false), all families fit the criteria (as no families are followed).
                if (request.IsFollowing.Value)
                {
                    return Result<PaginatedList<FamilyDto>>.Success(PaginatedList<FamilyDto>.Empty());
                }
                // Continue if IsFollowing is false, as all families are considered "not followed" for an unauthenticated user.
            }
            else // User is authenticated, proceed with filtering
            {
                var followedFamilyIds = _context.FamilyFollows
                    .Where(ff => ff.UserId == userId)
                    .Select(ff => ff.FamilyId)
                    .ToHashSet(); // Using HashSet for efficient lookups

            if (request.IsFollowing.Value)
            {
                // Filter to include only families that the current user is following
                query = query.Where(f => followedFamilyIds.Contains(f.Id));
            }
            else
            {
                // Filter to include only families that the current user is NOT following
                query = query.Where(f => !followedFamilyIds.Contains(f.Id));
            }
                    }
                } // Missing closing brace for 'if (request.IsFollowing.HasValue)'
                query = query.WithSpecification(new FamilySearchQuerySpecification(request.SearchQuery));
        query = query.WithSpecification(new FamilyOrderingSpecification(request.SortBy, request.SortOrder));
        query = query.WithSpecification(new FamilyAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));
        query = query.WithSpecification(new FamilyVisibilitySpecification(request.Visibility));

        var paginatedFamilyEntities = await query
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        var familyDtos = _mapper.Map<List<FamilyDto>>(paginatedFamilyEntities.Items);

        var filteredFamilyDtos = new List<FamilyDto>();
        foreach (var familyDto in familyDtos)
        {
            filteredFamilyDtos.Add(await _privacyService.ApplyPrivacyFilter(familyDto, familyDto.Id, cancellationToken));
        }

        var filteredPaginatedList = new PaginatedList<FamilyDto>(
            filteredFamilyDtos,
            paginatedFamilyEntities.TotalItems,
            paginatedFamilyEntities.Page,
            paginatedFamilyEntities.TotalPages
        );

        return Result<PaginatedList<FamilyDto>>.Success(filteredPaginatedList);
    }
}
