using backend.Application.Common.Extensions; // New using statement
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyDicts.Queries;

public record SearchFamilyDictsQuery : IRequest<PaginatedList<FamilyDictDto>>
{
    public string? Q { get; init; }
    public FamilyDictLineage? Lineage { get; init; }
    public string? Region { get; init; } // "north", "central", "south"
    public int Page { get; init; } = 1;
    public int ItemsPerPage { get; init; } = 10;
}

public class SearchFamilyDictsQueryHandler : IRequestHandler<SearchFamilyDictsQuery, PaginatedList<FamilyDictDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchFamilyDictsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FamilyDictDto>> Handle(SearchFamilyDictsQuery request, CancellationToken cancellationToken)
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

        return await query
            .ProjectTo<FamilyDictDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);
    }
}
