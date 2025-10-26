using MediatR;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;

namespace backend.Application.Common.Behaviours;

public class AuthenticationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUser _currentUser;

    public AuthenticationBehavior(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_currentUser.Id == null)
        {
            throw new UnauthorizedAccessException(ErrorMessages.Unauthorized);
        }

        return await next();
    }
}
