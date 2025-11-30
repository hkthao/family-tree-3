using backend.Domain.Common;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Giao diện định nghĩa một bộ điều phối sự kiện miền.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Điều phối và xóa tất cả các sự kiện miền từ một tập hợp các thực thể.
    /// </summary>
    /// <param name="entities">Tập hợp các thực thể có chứa sự kiện miền.</param>
    /// <returns>Một tác vụ đại diện cho hoạt động điều phối không đồng bộ.</returns>
    Task DispatchAndClearEvents(IEnumerable<BaseEntity> entities);
}
