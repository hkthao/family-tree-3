using backend.Domain.Common;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Giao diện cho dịch vụ xuất bản thông báo dựa trên các Domain Event.
/// </summary>
public interface IDomainEventNotificationPublisher
{
    /// <summary>
    /// Xuất bản một thông báo dựa trên Domain Event đã cho.
    /// </summary>
    /// <param name="domainEvent">Domain Event cần xử lý.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    Task PublishNotificationForEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
