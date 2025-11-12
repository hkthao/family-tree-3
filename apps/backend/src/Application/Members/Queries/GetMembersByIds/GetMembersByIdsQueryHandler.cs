using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetMembersByIdsQuery, Result<IReadOnlyList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IReadOnlyList<MemberListDto>>> Handle(GetMembersByIdsQuery request, CancellationToken cancellationToken)
    {
        var memberList = await _context.Members
            .WithSpecification(new MembersByIdsSpecification(request.Ids))
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MemberListDto>>.Success(memberList);
    }
}
