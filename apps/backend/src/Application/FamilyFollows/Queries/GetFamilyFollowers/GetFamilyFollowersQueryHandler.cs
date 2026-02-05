using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;

namespace backend.Application.FamilyFollows.Queries.GetFamilyFollowers;

public class GetFamilyFollowersQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetFamilyFollowersQuery, Result<ICollection<FamilyFollowDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ICollection<FamilyFollowDto>>> Handle(GetFamilyFollowersQuery request, CancellationToken cancellationToken)
    {
        var familyFollows = await _context.FamilyFollows
            .Where(ff => ff.FamilyId == request.FamilyId && ff.IsFollowing)
            .ProjectTo<FamilyFollowDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<ICollection<FamilyFollowDto>>.Success(familyFollows);
    }
}
