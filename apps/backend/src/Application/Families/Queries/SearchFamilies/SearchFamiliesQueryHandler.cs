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
                if (request.IsFollowing.Value)
                {
                    return Result<PaginatedList<FamilyDto>>.Success(PaginatedList<FamilyDto>.Empty());
                }
            }
            else
            {
                var followedFamilyIds = _context.FamilyFollows
                    .Where(ff => ff.UserId == userId && ff.IsFollowing) // Ensure IsFollowing is true for followed families
                    .Select(ff => ff.FamilyId)
                    .ToHashSet();

                if (request.IsFollowing.Value)
                {
                    query = query.Where(f => followedFamilyIds.Contains(f.Id));
                }
                else
                {
                    query = query.Where(f => !followedFamilyIds.Contains(f.Id));
                }
            }
        }
        query = query.WithSpecification(new FamilySearchQuerySpecification(request.SearchQuery));
        query = query.WithSpecification(new FamilyOrderingSpecification(request.SortBy, request.SortOrder));
        query = query.WithSpecification(new FamilyAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));
        query = query.WithSpecification(new FamilyVisibilitySpecification(request.Visibility));

        var paginatedFamilyEntities = await query
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        var familyDtos = _mapper.Map<List<FamilyDto>>(paginatedFamilyEntities.Items);

        if (_currentUser.IsAuthenticated)
        {
            var userId = _currentUser.UserId;
            var followedFamilyIds = (await _context.FamilyFollows
                .Where(ff => ff.UserId == userId && ff.IsFollowing)
                .Select(ff => ff.FamilyId)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            foreach (var familyDto in familyDtos)
            {
                familyDto.IsFollowing = followedFamilyIds.Contains(familyDto.Id);
            }
        }

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
