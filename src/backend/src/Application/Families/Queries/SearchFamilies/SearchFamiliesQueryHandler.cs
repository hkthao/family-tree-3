using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<SearchFamiliesQuery, Result<PaginatedList<FamilyDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<FamilyDto>>> Handle(SearchFamiliesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Families.AsQueryable();

        // Apply individual specifications
        query = query.WithSpecification(new FamilySearchTermSpecification(request.SearchQuery));
        query = query.WithSpecification(new FamilyOrderingSpecification(request.SortBy, request.SortOrder));
        query = query.WithSpecification(new FamilyVisibilitySpecification(request.Visibility));
        // Note: Pagination is handled by PaginatedListAsync, not a separate specification here.

        var paginatedList = await query
            .ProjectTo<FamilyDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<FamilyDto>>.Success(paginatedList);
    }
}
