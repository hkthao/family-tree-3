using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.CheckAiChatQuota;

/// <summary>
/// Truy vấn để kiểm tra xem hạn ngạch AI Chat của gia đình có bị vượt quá hay không.
/// </summary>
public record CheckAiChatQuotaQuery : IRequest<Result>
{
    /// <summary>
    /// ID của gia đình cần kiểm tra hạn ngạch.
    /// </summary>
    public Guid FamilyId { get; init; }
}
