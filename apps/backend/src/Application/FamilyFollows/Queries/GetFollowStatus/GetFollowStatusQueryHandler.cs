using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Security;

namespace backend.Application.FamilyFollows.Queries.GetFollowStatus;

[Authorize]
public class GetFollowStatusQueryHandler : IRequestHandler<GetFollowStatusQuery, Result<FamilyFollowDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public GetFollowStatusQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<FamilyFollowDto>> Handle(GetFollowStatusQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var familyFollow = await _context.FamilyFollows
            .Where(ff => ff.UserId == currentUserId && ff.FamilyId == request.FamilyId)
            .ProjectTo<FamilyFollowDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (familyFollow == null)
        {
            return Result<FamilyFollowDto>.Success(new FamilyFollowDto
            {
                UserId = currentUserId,
                FamilyId = request.FamilyId,
                IsFollowing = false,
                NotifyBirthday = false,
                NotifyDeathAnniversary = false,
                NotifyEvent = false
            });
        }

        return Result<FamilyFollowDto>.Success(familyFollow);
    }
}
