using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserPushTokens.DTOs;

namespace backend.Application.UserPushTokens.Queries.GetUserPushTokenById;

public class GetUserPushTokenByIdQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser) : IRequestHandler<GetUserPushTokenByIdQuery, Result<UserPushTokenDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<UserPushTokenDto>> Handle(GetUserPushTokenByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.UserPushTokens
            .Where(t => t.Id == request.Id && t.UserId == _currentUser.UserId)
            .ProjectTo<UserPushTokenDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<UserPushTokenDto>.Failure(string.Format(ErrorMessages.NotFound, $"UserPushToken with ID {request.Id}"), ErrorSources.NotFound);
        }

        return Result<UserPushTokenDto>.Success(entity);
    }
}
