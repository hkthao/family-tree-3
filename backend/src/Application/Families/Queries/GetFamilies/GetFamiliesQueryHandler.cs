using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added
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

    public async Task<Result<IReadOnlyList<FamilyListDto>>> Handle(GetFamiliesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Families.AsQueryable();

        // Apply individual specifications
        query = query.WithSpecification(new FamilySearchTermSpecification(request.SearchTerm));
        query = query.WithSpecification(new FamilyOrderingSpecification(request.SortBy, request.SortOrder));
        query = query.WithSpecification(new FamilyPaginationSpecification((request.Page - 1) * request.ItemsPerPage, request.ItemsPerPage));

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var familyList = await query
            .ProjectTo<FamilyListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<FamilyListDto>>.Success(familyList);
    }
}