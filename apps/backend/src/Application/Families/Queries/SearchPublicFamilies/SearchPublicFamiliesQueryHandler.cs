using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Families.Queries.SearchPublicFamilies;

public class SearchPublicFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService, ICurrentUser currentUser) : IRequestHandler<SearchPublicFamiliesQuery, Result<PaginatedList<FamilyDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;
    private readonly ICurrentUser _currentUser = currentUser; // NEW

    public async Task<Result<PaginatedList<FamilyDto>>> Handle(SearchPublicFamiliesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Family> query = _context.Families.AsNoTracking();

        // Apply FamilyVisibilitySpecification for public families
        query = query.WithSpecification(new FamilyVisibilitySpecification(FamilyVisibility.Public.ToString()));

        // Apply family following filter
        if (request.IsFollowing.HasValue)
        {
            var userId = _currentUser.UserId;
            if (!_currentUser.IsAuthenticated)
            {
                // If IsFollowing is true and user is not authenticated, no families are followed by this user
                if (request.IsFollowing.Value)
                {
                    return Result<PaginatedList<FamilyDto>>.Success(PaginatedList<FamilyDto>.Empty());
                }
                // If IsFollowing is false and user is not authenticated, no families are followed by this user, so all public families fit the criteria
            }
            else // User is authenticated, proceed with filtering
            {
                var followedFamilyIds = _context.FamilyFollows
                    .Where(ff => ff.UserId == userId)
                    .Select(ff => ff.FamilyId)
                    .ToHashSet();

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
        }

        // Apply FamilySearchQuerySpecification if a search query is provided
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            query = query.WithSpecification(new FamilySearchQuerySpecification(request.SearchQuery));
        }

        // Apply FamilyOrderingSpecification
        query = query.WithSpecification(new FamilyOrderingSpecification(request.SortBy, request.SortOrder));

        var paginatedFamilyEntities = await PaginatedList<Family>.CreateAsync(query, request.Page, request.ItemsPerPage);

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

