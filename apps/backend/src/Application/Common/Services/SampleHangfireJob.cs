using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Services;

public class SampleHangfireJob
{
    private readonly ILogger<SampleHangfireJob> _logger;

    public SampleHangfireJob(ILogger<SampleHangfireJob> logger)
    {
        _logger = logger;
    }

    public Task LogMessage(string message)
    {
        _logger.LogInformation($"Hangfire Sample Job Executed: {message}");
        return Task.CompletedTask;
    }
}
