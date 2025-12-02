using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser) : IRequestHandler<GetMembersByIdsQuery, Result<IReadOnlyList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<IReadOnlyList<MemberListDto>>> Handle(GetMembersByIdsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members
            .WithSpecification(new MemberAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));

        var memberList = await query
            .WithSpecification(new MembersByIdsSpecification(request.Ids)) // Apply IDs specification after access
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MemberListDto>>.Success(memberList);
    }
}
