using backend.Domain.ValueObjects;

namespace backend.Application.Common.Interfaces.Family;

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

    /// <summary>
    /// Đồng bộ hóa các sự kiện vòng đời của thành viên (ví dụ: sinh, mất).
    /// </summary>
    /// <param name="memberId">ID của thành viên.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="dateOfBirth">Ngày sinh của thành viên.</param>
    /// <param name="dateOfDeath">Ngày mất của thành viên.</param>
    /// <param name="lunarDateOfDeath">Ngày mất âm lịch của thành viên.</param>
    /// <param name="fullName">Tên đầy đủ của thành viên.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    Task SyncMemberLifeEvents(Guid memberId, Guid familyId, DateOnly? dateOfBirth, DateOnly? dateOfDeath, LunarDate? lunarDateOfDeath, string fullName, CancellationToken cancellationToken = default);
}
