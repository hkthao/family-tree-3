using backend.Application.Common.Interfaces;

namespace backend.Application.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandler : IRequestHandler<GetMembersByIdsQuery, List<MemberDto>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public GetMembersByIdsQueryHandler(IMemberRepository memberRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<List<MemberDto>> Handle(GetMembersByIdsQuery request, CancellationToken cancellationToken)
    {
        return (await _memberRepository.GetAllAsync())
            .Where(m => request.Ids.Contains(m.Id))
            .AsQueryable()
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .ToList();
    }
}
