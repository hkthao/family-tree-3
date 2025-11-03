using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.SearchMembers;

public class SearchMembersQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<SearchMembersQuery, Result<PaginatedList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<MemberListDto>>> Handle(SearchMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members.AsQueryable();

        // Apply individual specifications
        query = query.WithSpecification(new MemberSearchTermSpecification(request.SearchQuery));
        query = query.WithSpecification(new MemberByGenderSpecification(request.Gender));
        query = query.WithSpecification(new MemberByFamilyIdSpecification(request.FamilyId));
        query = query.WithSpecification(new MemberOrderingSpecification(request.SortBy, request.SortOrder));

        var paginatedList = await query
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<MemberListDto>>.Success(paginatedList);
    }
}
