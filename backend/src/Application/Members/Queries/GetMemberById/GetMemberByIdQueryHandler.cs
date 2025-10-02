using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDto>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public GetMemberByIdQueryHandler(IMemberRepository memberRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<MemberDto> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.Id);

        if (member == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Member), request.Id);
        }

        return _mapper.Map<MemberDto>(member);
    }
}
