using backend.Application.Common.Interfaces;
using backend.Application.Events.EventOccurrences.Jobs;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services;

public class HangfireJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireJobService> _logger;
    private readonly IDateTime _dateTime;
    private readonly IEventNotificationJob _eventNotificationJob; // Thêm IEventNotificationJob

    public HangfireJobService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<HangfireJobService> logger,
        IDateTime dateTime,
        IEventNotificationJob eventNotificationJob) // Inject IEventNotificationJob
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _logger = logger;
        _dateTime = dateTime;
        _eventNotificationJob = eventNotificationJob; // Khởi tạo
    }

    public string EnqueueGenerateEventOccurrences(int year, Guid? familyId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Enqueuing GenerateEventOccurrences job for year {year} and FamilyId {familyId}.");
        return _backgroundJobClient.Enqueue<GenerateEventOccurrencesJob>(
            job => job.GenerateOccurrences(year, familyId, CancellationToken.None)); // Hangfire injects its own CT
    }

    public void ScheduleGenerateEventOccurrencesAnnually()
    {
        _logger.LogInformation("Scheduling recurring GenerateEventOccurrences jobs annually for the next few years.");

        var currentYear = _dateTime.Now.Year;

        for (int year = currentYear; year <= currentYear + 5; year++)
        {
            _recurringJobManager.AddOrUpdate<GenerateEventOccurrencesJob>(
                $"generate-event-occurrences-{year}",
                job => job.GenerateOccurrences(year, null, CancellationToken.None), // Pass null for familyId, Hangfire injects its own CT
                Cron.Yearly(1, 1, 3, 0) // Month, Day, Hour, Minute
            );
        }
    }

    public void ScheduleEventNotifications()
    {
        _logger.LogInformation("Scheduling recurring EventNotificationJob daily.");

        // Chạy job thông báo sự kiện hàng ngày vào lúc 8:00 AM
        _recurringJobManager.AddOrUpdate<EventNotificationJob>(
            "event-notification-job",
            job => job.Run(CancellationToken.None),
            Cron.Daily(8, 0)); // Chạy hàng ngày lúc 8 AM
    }
}
