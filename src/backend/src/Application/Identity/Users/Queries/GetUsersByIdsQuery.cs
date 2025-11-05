using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Users.Queries;

public record GetUsersByIdsQuery(List<Guid> Ids) : IRequest<Result<List<UserDto>>>;

public class GetUsersByIdsQueryHandler(IApplicationDbContext context, ICurrentUser currentUserService) : IRequestHandler<GetUsersByIdsQuery, Result<List<UserDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUserService = currentUserService;

    public async Task<Result<List<UserDto>>> Handle(GetUsersByIdsQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Where(u => request.Ids.Contains(u.Id))
            .Select(u => new UserDto
            {
                Id = u.Id.ToString(),
                AuthProviderId = u.AuthProviderId,
                Email = u.Email,
                Name = u.Profile!.Name
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserDto>>.Success(users);
    }
}
