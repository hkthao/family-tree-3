using backend.Application.Common.Constants; // NEW
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.EventOccurrences.Jobs; // Needed for IGenerateEventOccurrencesJob
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.Commands.GenerateEventOccurrences;

public class GenerateEventOccurrencesCommandHandler : IRequestHandler<GenerateEventOccurrencesCommand, Result<string>>
{
    private readonly IGenerateEventOccurrencesJob _generateEventOccurrencesJob;
    private readonly ILogger<GenerateEventOccurrencesCommandHandler> _logger;
    private readonly IAuthorizationService _authorizationService; // NEW

    public GenerateEventOccurrencesCommandHandler(IGenerateEventOccurrencesJob generateEventOccurrencesJob, ILogger<GenerateEventOccurrencesCommandHandler> logger, IAuthorizationService authorizationService) // UPDATED
    {
        _generateEventOccurrencesJob = generateEventOccurrencesJob;
        _logger = logger;
        _authorizationService = authorizationService; // NEW
    }

    public async Task<Result<string>> Handle(GenerateEventOccurrencesCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!_authorizationService.IsAdmin())
        {
            _logger.LogWarning("Unauthorized attempt to generate event occurrences directly by non-admin user.");
            return Result<string>.Failure("Access Denied: Only administrators can generate event occurrences directly.", ErrorSources.Forbidden);
        }

        _logger.LogInformation($"Handling GenerateEventOccurrencesCommand for year {request.Year} and FamilyId {request.FamilyId}. Directly generating occurrences.");

        if (!request.FamilyId.HasValue)
        {
            _logger.LogWarning("GenerateEventOccurrencesCommand received with null FamilyId but direct generation requires a specific family. Please provide a FamilyId.");
            return Result<string>.Failure("Generating occurrences directly requires a specific FamilyId.");
        }

        await _generateEventOccurrencesJob.GenerateOccurrences(request.Year - 1, request.FamilyId.Value, cancellationToken);
        await _generateEventOccurrencesJob.GenerateOccurrences(request.Year, request.FamilyId.Value, cancellationToken);
        await _generateEventOccurrencesJob.GenerateOccurrences(request.Year + 1, request.FamilyId.Value, cancellationToken);

        return Result<string>.Success($"Event occurrences generated directly for year {request.Year} and FamilyId {request.FamilyId.Value}.");
    }
}
