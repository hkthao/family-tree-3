using backend.Application.Common.Interfaces;
using System.Threading.Channels;

namespace backend.Infrastructure.Services;

/// <summary>
/// Triển khai hàng đợi tác vụ nền sử dụng System.Threading.Channels.
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundTaskQueue(int capacity)
    {
        // BoundedChannelOptions để giới hạn kích thước hàng đợi, ngăn chặn việc sử dụng quá nhiều bộ nhớ.
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
    }

    /// <summary>
    /// Đưa một tác vụ vào hàng đợi.
    /// </summary>
    /// <param name="workItem">Tác vụ cần thực hiện.</param>
    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        _queue.Writer.WriteAsync(workItem);
    }

    /// <summary>
    /// Lấy một tác vụ từ hàng đợi.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Tác vụ từ hàng đợi.</returns>
    public async Task<Func<CancellationToken, Task>> DequeueBackgroundWorkItemAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
