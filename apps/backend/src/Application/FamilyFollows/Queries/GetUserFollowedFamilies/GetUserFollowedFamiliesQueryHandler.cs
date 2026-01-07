using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using backend.Application.Common.Security;
using backend.Application.Common.Models;

namespace backend.Application.FamilyFollows.Queries.GetUserFollowedFamilies;

[Authorize]
public class GetUserFollowedFamiliesQueryHandler : IRequestHandler<GetUserFollowedFamiliesQuery, Result<ICollection<FamilyFollowDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public GetUserFollowedFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<ICollection<FamilyFollowDto>>> Handle(GetUserFollowedFamiliesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var familyFollows = await _context.FamilyFollows
            .Where(ff => ff.UserId == currentUserId && ff.IsFollowing)
            .ProjectTo<FamilyFollowDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<ICollection<FamilyFollowDto>>.Success(familyFollows);
    }
}
