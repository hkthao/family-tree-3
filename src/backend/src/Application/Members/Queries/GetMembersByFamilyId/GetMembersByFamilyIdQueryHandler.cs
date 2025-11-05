using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers; // Added missing using directive

namespace backend.Application.Members.Queries.GetMembersByFamilyId;

public class GetMembersByFamilyIdQueryHandler : IRequestHandler<GetMembersByFamilyIdQuery, Result<List<MemberListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMembersByFamilyIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<MemberListDto>>> Handle(GetMembersByFamilyIdQuery request, CancellationToken cancellationToken)
    {
        var members = await _context.Members
            .Where(m => m.FamilyId == request.FamilyId)
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<MemberListDto>>.Success(members);
    }
}
