using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Specifications;

namespace backend.Application.Identity.UserProfiles.Queries.GetUserProfileByUserId;

public class GetUserProfileByUserIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetUserProfileByUserIdQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userProfileSpec = new UserProfileByUserIdSpec(request.UserId);
        var userProfile = await _context.UserProfiles
            .AsNoTracking()
            .WithSpecification(userProfileSpec)
            .FirstOrDefaultAsync(cancellationToken);

        if (userProfile == null)
        {
            return Result<UserProfileDto>.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);
        return Result<UserProfileDto>.Success(userProfileDto);
    }
}
