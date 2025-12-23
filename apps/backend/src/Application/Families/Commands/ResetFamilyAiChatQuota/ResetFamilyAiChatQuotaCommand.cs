using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.ResetFamilyAiChatQuota;

/// <summary>
/// Lệnh để đặt lại hạn ngạch trò chuyện AI hàng tháng của một gia đình.
/// </summary>
public record ResetFamilyAiChatQuotaCommand : IRequest<Result>
{
    /// <summary>
    /// ID của gia đình cần đặt lại hạn ngạch trò chuyện AI.
    /// </summary>
    public Guid FamilyId { get; init; }
}
