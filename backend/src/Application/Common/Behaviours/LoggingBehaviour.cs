using backend.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace backend.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
    {
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.Id; // This is ObjectId?
        string? userName = string.Empty;

        if (userId.HasValue) // Check if ObjectId has a value
        {
            userName = await _identityService.GetUserNameAsync(userId.Value); // Pass ObjectId.Value
        }

        _logger.LogInformation("backend Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId?.ToString(), userName, request); // Convert ObjectId? to string for logging
    }
}
