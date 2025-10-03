using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMembers;

namespace backend.Application.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandler : IRequestHandler<GetMembersByIdsQuery, IReadOnlyList<MemberListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMembersByIdsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MemberListDto>> Handle(GetMembersByIdsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Members
            .Where(m => request.Ids.Contains(m.Id))
            .Include(m => m.Relationships)
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}