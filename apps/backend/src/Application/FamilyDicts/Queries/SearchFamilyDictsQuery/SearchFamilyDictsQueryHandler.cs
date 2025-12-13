using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyDicts.Queries;

public class SearchFamilyDictsQueryHandler : IRequestHandler<SearchFamilyDictsQuery, Result<PaginatedList<FamilyDictDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchFamilyDictsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<FamilyDictDto>>> Handle(SearchFamilyDictsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FamilyDicts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            query = query.Where(r => r.Name.Contains(request.Q) || r.Description.Contains(request.Q));
        }

        if (request.Lineage.HasValue)
        {
            query = query.Where(r => r.Lineage == request.Lineage.Value);
        }

        // TODO: Implement region search logic (e.g., searching within NamesByRegion JSON)
        // This will require custom JSON querying or a different approach if NamesByRegion is complex.
        // For now, it's not implemented.

        var paginatedList = await query
            .ProjectTo<FamilyDictDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<FamilyDictDto>>.Success(paginatedList);
    }
}
