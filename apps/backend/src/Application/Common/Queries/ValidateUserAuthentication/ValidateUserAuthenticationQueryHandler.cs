using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Queries.ValidateUserAuthentication;

/// <summary>
/// Xử lý truy vấn để xác thực người dùng hiện tại có được xác thực hay không.
/// </summary>
public class ValidateUserAuthenticationQueryHandler : IRequestHandler<ValidateUserAuthenticationQuery, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ValidateUserAuthenticationQueryHandler> _logger;

    public ValidateUserAuthenticationQueryHandler(ICurrentUser currentUser, ILogger<ValidateUserAuthenticationQueryHandler> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public Task<Result> Handle(ValidateUserAuthenticationQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Người dùng chưa được xác thực.");
            return Task.FromResult(Result.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication));
        }

        _logger.LogInformation("Người dùng đã được xác thực.");
        return Task.FromResult(Result.Success());
    }
}
