using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Events.EventOccurrences.Jobs;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.Commands.GenerateAndNotifyEvents;

public class GenerateAndNotifyEventsCommandHandler(
    ILogger<GenerateAndNotifyEventsCommandHandler> logger,
    IAuthorizationService authorizationService,
    IGenerateEventOccurrencesJob generateEventOccurrencesJob,
    IEventNotificationJob eventNotificationJob,
    IDateTime dateTime) : IRequestHandler<GenerateAndNotifyEventsCommand, Result<string>>
{
    private readonly ILogger<GenerateAndNotifyEventsCommandHandler> _logger = logger;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IGenerateEventOccurrencesJob _generateEventOccurrencesJob = generateEventOccurrencesJob;
    private readonly IEventNotificationJob _eventNotificationJob = eventNotificationJob;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<string>> Handle(GenerateAndNotifyEventsCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!_authorizationService.IsAdmin())
        {
            _logger.LogWarning("Unauthorized attempt to generate and notify events by non-admin user.");
            return Result<string>.Failure("Access Denied: Only administrators can trigger this operation.", ErrorSources.Forbidden);
        }

        _logger.LogInformation("Starting GenerateAndNotifyEventsCommand handler for FamilyId: {FamilyId}, Year: {Year}", request.FamilyId, request.Year);

        var year = request.Year ?? _dateTime.Now.Year;

        // Run GenerateEventOccurrencesJob
        await _generateEventOccurrencesJob.GenerateOccurrences(year - 1, request.FamilyId, cancellationToken);
        await _generateEventOccurrencesJob.GenerateOccurrences(year, request.FamilyId, cancellationToken);
        await _generateEventOccurrencesJob.GenerateOccurrences(year + 1, request.FamilyId, cancellationToken);
        _logger.LogInformation("GenerateEventOccurrencesJob completed for FamilyId: {FamilyId}, Year: {Year}", request.FamilyId, year);

        // Run EventNotificationJob
        await _eventNotificationJob.Run(cancellationToken);
        _logger.LogInformation("EventNotificationJob completed.");

        return Result<string>.Success($"Successfully generated occurrences for year {year} and triggered notifications for FamilyId: {request.FamilyId}.");
    }
}
