namespace backend.Application.Common.Interfaces.Background;

/// <summary>
/// Defines an interface for scheduling background jobs, abstracting away the underlying job queue implementation.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueues a job to generate event occurrences for a specific year and optional family ID.
    /// </summary>
    /// <param name="year">The year for which to generate occurrences.</param>
    /// <param name="familyId">Optional: The ID of the family to generate occurrences for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    string EnqueueGenerateEventOccurrences(int year, Guid? familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules a recurring job to generate event occurrences annually for the next few years.
    /// </summary>
    void ScheduleGenerateEventOccurrencesAnnually();
}
