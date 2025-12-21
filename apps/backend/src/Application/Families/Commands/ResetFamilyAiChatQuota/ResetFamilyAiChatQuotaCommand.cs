using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Families.Commands.ResetFamilyAiChatQuota;

/// <summary>
/// Lệnh để đặt lại số lượng yêu cầu trò chuyện AI đã sử dụng hàng tháng cho một gia đình.
/// </summary>
public record ResetFamilyAiChatQuotaCommand : IRequest<Result>
{
    /// <summary>
    /// ID của gia đình cần đặt lại hạn mức.
    /// </summary>
    public Guid FamilyId { get; init; }
}
