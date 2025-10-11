using backend.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Behaviours
{
    public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
        where TRequest : notnull
    {
        private readonly ILogger _logger;
        private readonly IUser _user;

        public LoggingBehaviour(ILogger<TRequest> logger, IUser user)
        {
            _logger = logger;
            _user = user;
        }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _user.Id;

            _logger.LogInformation("backend Request: {Name} {@UserId} {@Request}",
                requestName, userId?.ToString(), request);

            return Task.CompletedTask;
        }
    }
}
