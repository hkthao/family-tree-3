using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums; // For FamilyDictLineage

namespace backend.Application.FamilyDicts.Queries.Public;

public record GetPublicFamilyDictsQuery : IRequest<PaginatedList<FamilyDictDto>>
{
    public int Page { get; init; } = 1;
    public int ItemsPerPage { get; init; } = 10;
    public string? SearchQuery { get; init; }
    public FamilyDictLineage? Lineage { get; init; }
    public string? Region { get; init; } // e.g., North, Central, South
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; } // "asc" or "desc"
}

public class GetPublicFamilyDictsQueryHandler : IRequestHandler<GetPublicFamilyDictsQuery, PaginatedList<FamilyDictDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPublicFamilyDictsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FamilyDictDto>> Handle(GetPublicFamilyDictsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FamilyDicts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            query = query.Where(f => f.Name.Contains(request.SearchQuery) || f.Description.Contains(request.SearchQuery));
        }

        if (request.Lineage.HasValue)
        {
            query = query.Where(f => f.Lineage == request.Lineage.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Region))
        {
            var searchRegion = request.Region;
            query = query.Where(f =>
                f.NamesByRegion.North != null && f.NamesByRegion.North.Contains(searchRegion));
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            // Apply sorting dynamically
            query = query.OrderByPropertyName(request.SortBy, request.SortOrder == "desc");
        }
        else
        {
            // Default sorting if no SortBy is provided
            query = query.OrderBy(f => f.Name);
        }

        return await query
            .ProjectTo<FamilyDictDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);
    }
}
