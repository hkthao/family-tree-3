using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Mappings;

namespace backend.Application.Members.Queries.SearchMembers;

public class SearchMembersQueryHandler : IRequestHandler<SearchMembersQuery, PaginatedList<MemberDto>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public SearchMembersQueryHandler(IMemberRepository memberRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MemberDto>> Handle(SearchMembersQuery request, CancellationToken cancellationToken)
    {
        var query = (await _memberRepository.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(m => m.FirstName.Contains(request.Keyword) || m.LastName.Contains(request.Keyword) || (m.Biography != null && m.Biography.Contains(request.Keyword)));
        }

        return await query
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
