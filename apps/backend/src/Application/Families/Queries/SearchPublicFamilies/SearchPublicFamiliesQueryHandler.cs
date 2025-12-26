using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Families.Queries.SearchPublicFamilies;

public class SearchPublicFamiliesQueryHandler : IRequestHandler<SearchPublicFamiliesQuery, Result<PaginatedList<FamilyDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchPublicFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<FamilyDto>>> Handle(SearchPublicFamiliesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Families
            .AsNoTracking()
            .Where(f => f.Visibility == FamilyVisibility.Public.ToString());

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var searchQuery = request.SearchQuery;
            query = query.Where(f => f.Name.Contains(searchQuery) || (f.Description != null && f.Description.Contains(searchQuery)));
        }

        var paginatedList = await PaginatedList<Family>.CreateAsync(query, request.Page, request.ItemsPerPage);

        return Result<PaginatedList<FamilyDto>>.Success(_mapper.Map<PaginatedList<FamilyDto>>(paginatedList));
    }
}
