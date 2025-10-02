using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandler : IRequestHandler<GetMembersByIdsQuery, List<MemberDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMembersByIdsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MemberDto>> Handle(GetMembersByIdsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Members
            .Where(m => request.Ids.Contains(m.Id))
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
