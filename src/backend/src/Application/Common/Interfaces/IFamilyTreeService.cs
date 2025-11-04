namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho dịch vụ quản lý cây gia phả.
/// </summary>
public interface IFamilyTreeService
{
    /// <summary>
    /// Cập nhật các số liệu thống kê của gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    Task UpdateFamilyStats(Guid familyId, CancellationToken cancellationToken = default);
}
