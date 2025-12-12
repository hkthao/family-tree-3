using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Users.Queries;

public class IsFamilyManagerQueryHandler(IAuthorizationService authorizationService) : IRequestHandler<IsFamilyManagerQuery, Result<bool>>
{
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<bool>> Handle(IsFamilyManagerQuery request, CancellationToken cancellationToken)
    {
        var isManager = _authorizationService.CanManageFamily(request.FamilyId);
        return await Task.FromResult(Result<bool>.Success(isManager));
    }
}
