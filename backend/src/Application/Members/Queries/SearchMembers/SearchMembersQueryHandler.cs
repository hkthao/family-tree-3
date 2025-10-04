using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Mappings;

using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.SearchMembers;

public class SearchMembersQueryHandler : IRequestHandler<SearchMembersQuery, PaginatedList<MemberListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchMembersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MemberListDto>> Handle(SearchMembersQuery request, CancellationToken cancellationToken)
    {
        var spec = new MemberFilterSpecification(request);

        var query = _context.Members.AsQueryable().WithSpecification(spec);

        return await query
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);
    }
}
