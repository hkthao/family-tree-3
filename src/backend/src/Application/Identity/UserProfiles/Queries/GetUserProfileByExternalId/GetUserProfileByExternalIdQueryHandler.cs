using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Identity.UserProfiles.Queries.GetUserProfileByExternalId;

public class GetUserProfileByExternalIdQueryHandler : IRequestHandler<GetUserProfileByExternalIdQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserProfileByExternalIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileByExternalIdQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.ExternalId == request.ExternalId, cancellationToken);

        if (userProfile == null)
        {
            return Result<UserProfileDto>.Failure("User profile not found.", "NotFound");
        }

        var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);
        return Result<UserProfileDto>.Success(userProfileDto);
    }
}
