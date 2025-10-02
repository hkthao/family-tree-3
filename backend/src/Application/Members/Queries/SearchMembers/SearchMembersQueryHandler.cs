using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Mappings;

namespace backend.Application.Members.Queries.SearchMembers;

public class SearchMembersQueryHandler : IRequestHandler<SearchMembersQuery, PaginatedList<MemberDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchMembersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MemberDto>> Handle(SearchMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(m => m.FirstName.Contains(request.Keyword) || m.LastName.Contains(request.Keyword) || (m.Biography != null && m.Biography.Contains(request.Keyword)));
        }

        return await query
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
