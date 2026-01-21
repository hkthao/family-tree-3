using backend.Application.Common.Interfaces;
using backend.Application.Common.Services;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.EventOccurrences.Jobs;

public class GenerateEventOccurrencesJob : IGenerateEventOccurrencesJob
{
    private readonly ILogger<GenerateEventOccurrencesJob> _logger;
    private readonly IApplicationDbContext _context;
    private readonly ILunarCalendarService _lunarCalendarService;
    private readonly IDateTime _dateTime;

    public GenerateEventOccurrencesJob(
        ILogger<GenerateEventOccurrencesJob> logger,
        IApplicationDbContext context,
        ILunarCalendarService lunarCalendarService,
        IDateTime dateTime)
    {
        _logger = logger;
        _context = context;
        _lunarCalendarService = lunarCalendarService;
        _dateTime = dateTime;
    }

    public async Task GenerateOccurrences(int year, Guid? familyId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Hangfire Job: Starting to generate event occurrences for year {year} (FamilyId: {familyId}).");

        const int BatchSize = 200;
        int skipCount = 0;
        bool moreEvents = true;
        int totalGenerated = 0;

        while (moreEvents)
        {
            var eventsQuery = _context.Events
                .Where(e => !e.IsDeleted && e.CalendarType == Domain.Enums.CalendarType.Lunar && e.RepeatRule == Domain.Enums.RepeatRule.Yearly);

            if (familyId.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.FamilyId == familyId.Value);
            }

            var eventsToProcessBatch = await eventsQuery
                .OrderBy(e => e.Id) // Ensure consistent ordering for pagination
                .Skip(skipCount)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (!eventsToProcessBatch.Any())
            {
                moreEvents = false; // No more events to process
                break;
            }

            var eventIdsInBatch = eventsToProcessBatch.Select(e => e.Id).ToHashSet();

            // Efficiently query existing occurrences for the current batch
            var existingOccurrences = (await _context.EventOccurrences
                .Where(eo => eo.Year == year && eventIdsInBatch.Contains(eo.EventId))
                .Select(eo => eo.EventId)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            var newEventOccurrences = new List<EventOccurrence>();

            foreach (var evt in eventsToProcessBatch)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip if occurrence already exists for this event and year
                if (existingOccurrences.Contains(evt.Id))
                {
                    _logger.LogInformation($"Occurrence for Event ID {evt.Id} in {year} already exists. Skipping event {evt.Id}.");
                    continue;
                }

                if (evt.LunarDate != null && evt.LunarDate.Day.HasValue && evt.LunarDate.Month.HasValue)
                {
                    int lunarDay = evt.LunarDate.Day.Value;
                    int lunarMonth = evt.LunarDate.Month.Value;
                    bool isLeapMonth = evt.LunarDate.IsLeapMonth.GetValueOrDefault(false);

                    DateTime? solarOccurrenceDate = _lunarCalendarService.ConvertLunarToSolar(lunarDay, lunarMonth, year, isLeapMonth);

                    if (solarOccurrenceDate.HasValue)
                    {
                        var newOccurrence = EventOccurrence.Create(evt.Id, year, solarOccurrenceDate.Value);
                        newEventOccurrences.Add(newOccurrence);
                        _logger.LogInformation($"Prepared occurrence for Event ID {evt.Id} in {year}: {solarOccurrenceDate.Value:yyyy-MM-dd}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to convert lunar date for Event ID {evt.Id} (Lunar: {evt.LunarDate}) in year {year}. Skipping.");
                    }
                }
            }

            if (newEventOccurrences.Any())
            {
                _context.EventOccurrences.AddRange(newEventOccurrences);
                await _context.SaveChangesAsync(cancellationToken);
                totalGenerated += newEventOccurrences.Count;
                _logger.LogInformation($"Batch processed: Saved {newEventOccurrences.Count} new event occurrences for year {year}. Total generated: {totalGenerated}.");
            }
            else
            {
                _logger.LogInformation($"Batch processed: No new event occurrences to save for year {year} in this batch.");
            }

            skipCount += BatchSize;
        }

        _logger.LogInformation($"Hangfire Job: Finished generating event occurrences for year {year}. Total new occurrences generated: {totalGenerated}.");
    }
}
