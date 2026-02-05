using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.Queries.CheckAiChatQuota;

/// <summary>
/// Xử lý truy vấn để kiểm tra hạn ngạch AI Chat của gia đình.
/// </summary>
public class CheckAiChatQuotaQueryHandler : IRequestHandler<CheckAiChatQuotaQuery, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CheckAiChatQuotaQueryHandler> _logger;

    public CheckAiChatQuotaQueryHandler(IApplicationDbContext context, ILogger<CheckAiChatQuotaQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(CheckAiChatQuotaQuery request, CancellationToken cancellationToken)
    {
        var family = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            _logger.LogWarning("Không tìm thấy gia đình với ID: {FamilyId}.", request.FamilyId);
            return Result.NotFound(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        if (family.FamilyLimitConfiguration == null)
        {
            _logger.LogError("FamilyLimitConfiguration không tồn tại cho FamilyId {FamilyId} trong khi kiểm tra hạn ngạch. Điều này không nên xảy ra.", request.FamilyId);
            return Result.Failure("Cấu hình giới hạn AI không tồn tại cho gia đình này.", ErrorSources.InvalidConfiguration);
        }

        if (family.FamilyLimitConfiguration.AiChatMonthlyUsage >= family.FamilyLimitConfiguration.AiChatMonthlyLimit)
        {
            _logger.LogWarning("AI Chat quota exceeded for FamilyId {FamilyId}. Usage: {Usage}, Limit: {Limit}",
                request.FamilyId, family.FamilyLimitConfiguration.AiChatMonthlyUsage, family.FamilyLimitConfiguration.AiChatMonthlyLimit);
            return Result.Failure(ErrorMessages.AiChatQuotaExceeded, ErrorSources.QuotaExceeded);
        }

        _logger.LogInformation("AI Chat quota OK for FamilyId {FamilyId}. Usage: {Usage}, Limit: {Limit}",
            request.FamilyId, family.FamilyLimitConfiguration.AiChatMonthlyUsage, family.FamilyLimitConfiguration.AiChatMonthlyLimit);
        return Result.Success();
    }
}
