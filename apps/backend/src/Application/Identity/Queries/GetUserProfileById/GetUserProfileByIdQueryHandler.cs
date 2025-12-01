using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Specifications;

namespace backend.Application.Identity.UserProfiles.Queries.GetUserProfileById;

public class GetUserProfileByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetUserProfileByIdQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var userProfileSpec = new UserProfileByIdSpecification(request.Id);
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
