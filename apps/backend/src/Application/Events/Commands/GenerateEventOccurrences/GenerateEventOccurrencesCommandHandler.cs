using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.EventOccurrences.Jobs; // Needed for IGenerateEventOccurrencesJob
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.Events.Commands.GenerateEventOccurrences;

public class GenerateEventOccurrencesCommandHandler : IRequestHandler<GenerateEventOccurrencesCommand, Result<string>>
{
    private readonly IGenerateEventOccurrencesJob _generateEventOccurrencesJob; // Inject the interface
    private readonly ILogger<GenerateEventOccurrencesCommandHandler> _logger;

    public GenerateEventOccurrencesCommandHandler(IGenerateEventOccurrencesJob generateEventOccurrencesJob, ILogger<GenerateEventOccurrencesCommandHandler> logger)
    {
        _generateEventOccurrencesJob = generateEventOccurrencesJob;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(GenerateEventOccurrencesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handling GenerateEventOccurrencesCommand for year {request.Year} and FamilyId {request.FamilyId}. Directly generating occurrences.");

        if (!request.FamilyId.HasValue)
        {
            _logger.LogWarning("GenerateEventOccurrencesCommand received with null FamilyId but direct generation requires a specific family. Please provide a FamilyId.");
            return Result<string>.Failure("Generating occurrences directly requires a specific FamilyId.");
        }

        await _generateEventOccurrencesJob.GenerateOccurrences(request.Year, request.FamilyId.Value, cancellationToken);

        return Result<string>.Success($"Event occurrences generated directly for year {request.Year} and FamilyId {request.FamilyId.Value}.");
    }
}