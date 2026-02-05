using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;

namespace backend.Application.Common.Behaviours;

public class AuthenticationBehavior<TRequest, TResponse>(ICurrentUser currentUser) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(ErrorMessages.Unauthorized);
        }

        return await next();
    }
}
