using backend.Application.Common.Interfaces.Background;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services.Background;

/// <summary>
/// Dịch vụ lưu trữ chạy các tác vụ nền từ hàng đợi.
/// </summary>
public class QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ILogger<QueuedHostedService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IBackgroundTaskQueue TaskQueue { get; } = taskQueue;

    /// <summary>
    /// Logic chính để thực thi các tác vụ nền.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ComponentName} is running.", nameof(QueuedHostedService));

        await ProcessTaskQueueAsync(cancellationToken);
    }

    /// <summary>
    /// Xử lý hàng đợi tác vụ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</n    
    private async Task ProcessTaskQueueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Func<CancellationToken, Task> workItem = await TaskQueue.DequeueBackgroundWorkItemAsync(cancellationToken);

            try
            {
                using (var scope = _serviceProvider.CreateScope()) // Create a new scope for each work item
                {
                    // Execute the work item within the scope
                    await workItem(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // When the service is stopping, DequeueBackgroundWorkItemAsync will throw OperationCanceledException.
                _logger.LogInformation("{ComponentName} is stopping.", nameof(QueuedHostedService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing work item in {ComponentName}.", nameof(QueuedHostedService));
            }
        }
    }

    /// <summary>
    /// Được gọi khi dịch vụ lưu trữ đang dừng.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ComponentName} is stopping.", nameof(QueuedHostedService));

        await base.StopAsync(cancellationToken);
    }
}
