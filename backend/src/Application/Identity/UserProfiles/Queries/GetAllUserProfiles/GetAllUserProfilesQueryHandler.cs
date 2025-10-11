using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Identity.UserProfiles.Queries.GetAllUserProfiles
{
    public class GetAllUserProfilesQueryHandler : IRequestHandler<GetAllUserProfilesQuery, Result<List<UserProfileDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllUserProfilesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<UserProfileDto>>> Handle(GetAllUserProfilesQuery request, CancellationToken cancellationToken)
        {
            var userProfiles = await _context.UserProfiles
                .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<UserProfileDto>>.Success(userProfiles);
        }
    }
}
