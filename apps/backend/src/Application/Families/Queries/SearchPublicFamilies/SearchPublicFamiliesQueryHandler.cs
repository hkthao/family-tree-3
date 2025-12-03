using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.Enums;

namespace backend.Application.Families.Queries.SearchPublicFamilies;

/// <summary>
/// Xử lý truy vấn để tìm kiếm các gia đình công khai.
/// </summary>
public class SearchPublicFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<SearchPublicFamiliesQuery, Result<PaginatedList<FamilyDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<FamilyDto>>> Handle(SearchPublicFamiliesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Families.AsQueryable();

        // Chỉ bao gồm các gia đình có Visibility là Public
        query = query.WithSpecification(new FamilyVisibilitySpecification(FamilyVisibility.Public.ToString()));

        // Áp dụng các tiêu chí tìm kiếm khác
        query = query.WithSpecification(new FamilySearchTermSpecification(request.SearchTerm));
        query = query.WithSpecification(new FamilyOrderingSpecification(request.SortBy, request.SortOrder));

        var paginatedList = await query
            .ProjectTo<FamilyDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<FamilyDto>>.Success(paginatedList);
    }
}
