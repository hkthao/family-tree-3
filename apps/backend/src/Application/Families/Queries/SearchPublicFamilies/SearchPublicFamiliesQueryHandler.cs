using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Families.Queries.SearchPublicFamilies;

public class SearchPublicFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService) : IRequestHandler<SearchPublicFamiliesQuery, Result<PaginatedList<FamilyDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<PaginatedList<FamilyDto>>> Handle(SearchPublicFamiliesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Family> query = _context.Families.AsNoTracking();

        // Apply FamilyVisibilitySpecification for public families
        query = query.WithSpecification(new FamilyVisibilitySpecification(FamilyVisibility.Public.ToString()));

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

