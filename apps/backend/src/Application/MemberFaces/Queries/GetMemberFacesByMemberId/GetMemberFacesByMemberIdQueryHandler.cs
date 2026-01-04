using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;

namespace backend.Application.MemberFaces.Queries.GetMemberFacesByMemberId;

public class GetMemberFacesByMemberIdQueryHandler : IRequestHandler<GetMemberFacesByMemberIdQuery, Result<IEnumerable<MemberFaceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMemberFacesByMemberIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<MemberFaceDto>>> Handle(GetMemberFacesByMemberIdQuery request, CancellationToken cancellationToken)
    {
        var memberFaces = await _context.MemberFaces
            .Include(e=>e.Member)
            .Where(mf => mf.MemberId == request.MemberId)
            .OrderBy(mf => mf.Created)
            .ToListAsync(cancellationToken);

        var memberFaceDtos = _mapper.Map<IEnumerable<MemberFaceDto>>(memberFaces);

        return Result<IEnumerable<MemberFaceDto>>.Success(memberFaceDtos);
    }
}
