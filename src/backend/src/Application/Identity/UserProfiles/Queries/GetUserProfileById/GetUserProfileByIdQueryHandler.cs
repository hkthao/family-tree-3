using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Identity.UserProfiles.Queries.GetUserProfileById;

public class GetUserProfileByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetUserProfileByIdQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.Id == request.Id, cancellationToken);

        if (userProfile == null)
        {
            return Result<UserProfileDto>.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);
        return Result<UserProfileDto>.Success(userProfileDto);
    }
}
