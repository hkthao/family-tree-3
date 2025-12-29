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
