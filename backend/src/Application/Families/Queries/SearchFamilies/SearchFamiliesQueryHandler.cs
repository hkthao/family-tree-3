using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Mappings;

namespace backend.Application.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandler : IRequestHandler<SearchFamiliesQuery, Result<PaginatedList<FamilyDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<FamilyDto>>> Handle(SearchFamiliesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Families.AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchQuery))
        {
            query = query.Where(f => f.Name.Contains(request.SearchQuery) || (f.Description != null && f.Description.Contains(request.SearchQuery)));
        }

        if (!string.IsNullOrEmpty(request.Visibility))
        {
            query = query.Where(f => f.Visibility == request.Visibility);
        }

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            switch (request.SortBy.ToLower())
            {
                case "name":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(f => f.Name) : query.OrderBy(f => f.Name);
                    break;
                case "totalmembers":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(f => f.TotalMembers) : query.OrderBy(f => f.TotalMembers);
                    break;
                case "created":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(f => f.Created) : query.OrderBy(f => f.Created);
                    break;
                default:
                    query = query.OrderBy(f => f.Name); // Default sort
                    break;
            }
        }
        else
        {
            query = query.OrderBy(f => f.Name); // Default sort if no sortBy is provided
        }

        var paginatedList = await query
            .ProjectTo<FamilyDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<FamilyDto>>.Success(paginatedList);
    }
}
