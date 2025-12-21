using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Families.Commands.UpdateFamilyLimitConfiguration;

/// <summary>
/// Command để cập nhật cấu hình giới hạn của một gia đình.
/// </summary>
public record UpdateFamilyLimitConfigurationCommand : IRequest<Result>
{
    /// <summary>
    /// ID của gia đình cần cập nhật cấu hình.
    /// </summary>
    public Guid FamilyId { get; init; }

    /// <summary>
    /// Số lượng thành viên tối đa được phép trong gia đình.
    /// </summary>
    public int MaxMembers { get; init; }

    /// <summary>
    /// Dung lượng lưu trữ tối đa được cấp cho gia đình (tính bằng Megabyte).
    /// </summary>
    public int MaxStorageMb { get; init; }

    /// <summary>
    /// Giới hạn số lượng yêu cầu trò chuyện AI mỗi tháng cho gia đình.
    /// </summary>
    public int AiChatMonthlyLimit { get; init; }
}
