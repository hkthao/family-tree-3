using backend.Application.Common.Interfaces.Core;
using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Behaviours;

/// <summary>
/// Hành vi pipeline để ghi log thông tin yêu cầu trước khi xử lý.
/// </summary>
/// <typeparam name="TRequest">Kiểu của yêu cầu.</typeparam>
public class LoggingBehaviour<TRequest, TResponse>(ILogger<TRequest> logger, ICurrentUser user) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger _logger = logger;
    private readonly ICurrentUser _user = user;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.UserId;

        _logger.LogInformation("backend Request: {Name} {@UserId} {@Request}",
            requestName, userId, request);

        var response = await next();

        return response;
    }
}
