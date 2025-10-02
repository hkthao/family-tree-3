using backend.Application.Common.Interfaces;

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, List<MemberDto>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public GetMembersQueryHandler(IMemberRepository memberRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<List<MemberDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _memberRepository.GetAllAsync();
        return _mapper.Map<List<MemberDto>>(members);
    }
}
