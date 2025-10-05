using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandler : IRequestHandler<GetFamiliesQuery, Result<IReadOnlyList<FamilyListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<FamilyListDto>> Handle(GetFamiliesQuery request, CancellationToken cancellationToken)
    {
        var spec = new FamilyFilterSpecification(
            request.SearchTerm,
            (request.Page - 1) * request.ItemsPerPage,
            request.ItemsPerPage,
            request.SortBy,
            request.SortOrder);

        // Comment: Specification pattern is applied here to filter, sort, and page the results at the database level.
        var query = _context.Families.AsQueryable().WithSpecification(spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var familyList = await query
            .ProjectTo<FamilyListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<FamilyListDto>>.Success(familyList);
    }
}