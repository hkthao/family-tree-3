using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.SearchMembers;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersWithPaginationQueryHandler : IRequestHandler<GetMembersWithPaginationQuery, PaginatedList<MemberListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMembersWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MemberListDto>> Handle(GetMembersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var searchQuery = new SearchMembersQuery
        {
            SearchQuery = request.SearchTerm,
            FamilyId = request.FamilyId,
            Page = request.Page,
            ItemsPerPage = request.ItemsPerPage
        };

        var spec = new MemberFilterSpecification(searchQuery);

        // Comment: Specification pattern is applied here to filter, sort, and page the results at the database level.
        var query = _context.Members.AsQueryable().WithSpecification(spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        return await PaginatedList<MemberListDto>.CreateAsync(query.ProjectTo<MemberListDto>(_mapper.ConfigurationProvider).AsNoTracking(), request.Page, request.ItemsPerPage);
    }
}