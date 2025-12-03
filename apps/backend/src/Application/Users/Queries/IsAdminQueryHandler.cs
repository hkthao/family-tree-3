using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Users.Queries;

public class IsAdminQueryHandler(IAuthorizationService authorizationService) : IRequestHandler<IsAdminQuery, Result<bool>>
{
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<bool>> Handle(IsAdminQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = _authorizationService.IsAdmin();
        return await Task.FromResult(Result<bool>.Success(isAdmin));
    }
}
