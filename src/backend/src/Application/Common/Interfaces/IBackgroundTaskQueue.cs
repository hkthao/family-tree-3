using System;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Giao diện cho một hàng đợi tác vụ nền không đồng bộ.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Đưa một tác vụ vào hàng đợi để xử lý không đồng bộ.
    /// </summary>
    /// <param name="workItem">Hàm đại diện cho tác vụ cần thực hiện.</param>
    void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

    /// <summary>
    /// Lấy một tác vụ từ hàng đợi để xử lý.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Hàm đại diện cho tác vụ cần thực hiện.</returns>
    Task<Func<CancellationToken, Task>> DequeueBackgroundWorkItemAsync(CancellationToken cancellationToken);
}
