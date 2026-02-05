using backend.Application.Common.Interfaces.Background;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.Commands.RescheduleRecurringEventOccurrences;

public class RescheduleRecurringEventOccurrencesCommandHandler(IBackgroundJobService backgroundJobService, ILogger<RescheduleRecurringEventOccurrencesCommandHandler> logger) : IRequestHandler<RescheduleRecurringEventOccurrencesCommand, Result<string>>
{
    private readonly IBackgroundJobService _backgroundJobService = backgroundJobService;
    private readonly ILogger<RescheduleRecurringEventOccurrencesCommandHandler> _logger = logger;

    public Task<Result<string>> Handle(RescheduleRecurringEventOccurrencesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling RescheduleRecurringEventOccurrencesCommand.");
        _backgroundJobService.ScheduleGenerateEventOccurrencesAnnually();
        return Task.FromResult(Result<string>.Success("Recurring event occurrence generation jobs rescheduled."));
    }
}
