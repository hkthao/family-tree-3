using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.IncrementFamilyAiChatUsage;

/// <summary>
/// Lệnh để tăng số lượng yêu cầu trò chuyện AI đã sử dụng hàng tháng cho một gia đình.
/// </summary>
public record IncrementFamilyAiChatUsageCommand : IRequest<Result>
{
    /// <summary>
    /// ID của gia đình cần tăng hạn mức sử dụng AI.
    /// </summary>
    public Guid FamilyId { get; init; }
}
