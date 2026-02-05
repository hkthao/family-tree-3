using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore; // New import
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.FamilyLinks.Specifications; // New import
using backend.Domain.Entities;

namespace backend.Application.FamilyLinks.Queries.SearchFamilyLinks;

public class SearchFamilyLinksQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService) : IRequestHandler<SearchFamilyLinksQuery, Result<PaginatedList<FamilyLinkDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<PaginatedList<FamilyLinkDto>>> Handle(SearchFamilyLinksQuery request, CancellationToken cancellationToken)
    {
        // 1. Authorization: User must be a member of the family to view its links
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<PaginatedList<FamilyLinkDto>>.Forbidden("Bạn không có quyền xem các liên kết của gia đình này.");
        }

        // 2. Build Query using Specifications
        var query = _context.FamilyLinks.AsQueryable().AsNoTracking(); // Start with IQueryable

        // Apply FamilyLinkByFamilyIdSpecification
        var familyIdSpecification = new FamilyLinkByFamilyIdSpecification(request.FamilyId);
        query = query.WithSpecification(familyIdSpecification);

        // Conditionally apply FamilyLinkBySearchQuerySpecification
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var searchQuerySpecification = new FamilyLinkBySearchQuerySpecification(request.SearchQuery);
            query = query.WithSpecification(searchQuerySpecification);
        }

        // Conditionally apply FamilyLinkByOtherFamilyIdSpecification
        if (request.OtherFamilyId.HasValue)
        {
            var otherFamilyIdSpecification = new FamilyLinkByOtherFamilyIdSpecification(request.OtherFamilyId.Value);
            query = query.WithSpecification(otherFamilyIdSpecification);
        }

        // 3. Apply Sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query = request.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(request.SortBy))
                : query.OrderBy(GetSortProperty(request.SortBy));
        }
        else
        {
            // Default sort if none specified
            query = query.OrderBy(fl => fl.LinkDate);
        }

        // 4. Project to DTO and Paginate
        var paginatedList = await PaginatedList<FamilyLinkDto>.CreateAsync(
            query.ProjectTo<FamilyLinkDto>(_mapper.ConfigurationProvider).AsNoTracking(),
            request.Page,
            request.ItemsPerPage
        );

        return Result<PaginatedList<FamilyLinkDto>>.Success(paginatedList);
    }

    private System.Linq.Expressions.Expression<Func<FamilyLink, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "linkdate" => familyLink => familyLink.LinkDate,
            _ => familyLink => familyLink.LinkDate, // Default sort
        };
    }
}
