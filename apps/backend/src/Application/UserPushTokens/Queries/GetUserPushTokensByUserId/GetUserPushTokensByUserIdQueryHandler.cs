using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserPushTokens.DTOs;

namespace backend.Application.UserPushTokens.Queries.GetUserPushTokensByUserId;

public class GetUserPushTokensByUserIdQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser) : IRequestHandler<GetUserPushTokensByUserIdQuery, Result<List<UserPushTokenDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<List<UserPushTokenDto>>> Handle(GetUserPushTokensByUserIdQuery request, CancellationToken cancellationToken)
    {
        // For security, ensure the requested UserId matches the current authenticated user's ID
        // unless an administrator is making the request.
        if (_currentUser.UserId != request.UserId)
        {
            return Result<List<UserPushTokenDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entities = await _context.UserPushTokens
            .Where(t => t.UserId == request.UserId)
            .ProjectTo<UserPushTokenDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<UserPushTokenDto>>.Success(entities);
    }
}
