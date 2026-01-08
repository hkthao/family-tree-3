using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.Commands.RescheduleRecurringEventOccurrences;

public class RescheduleRecurringEventOccurrencesCommandHandler : IRequestHandler<RescheduleRecurringEventOccurrencesCommand, Result<string>>
{
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<RescheduleRecurringEventOccurrencesCommandHandler> _logger;

    public RescheduleRecurringEventOccurrencesCommandHandler(IBackgroundJobService backgroundJobService, ILogger<RescheduleRecurringEventOccurrencesCommandHandler> logger)
    {
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    public Task<Result<string>> Handle(RescheduleRecurringEventOccurrencesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling RescheduleRecurringEventOccurrencesCommand.");
        _backgroundJobService.ScheduleGenerateEventOccurrencesAnnually();
        return Task.FromResult(Result<string>.Success("Recurring event occurrence generation jobs rescheduled."));
    }
}
