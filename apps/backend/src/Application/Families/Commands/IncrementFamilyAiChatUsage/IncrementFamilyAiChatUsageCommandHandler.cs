using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.Commands.IncrementFamilyAiChatUsage;

/// <summary>
/// Handler cho IncrementFamilyAiChatUsageCommand.
/// </summary>
public class IncrementFamilyAiChatUsageCommandHandler : IRequestHandler<IncrementFamilyAiChatUsageCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<IncrementFamilyAiChatUsageCommandHandler> _logger;

    public IncrementFamilyAiChatUsageCommandHandler(IApplicationDbContext context, ILogger<IncrementFamilyAiChatUsageCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(IncrementFamilyAiChatUsageCommand request, CancellationToken cancellationToken)
    {
        var family = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result.NotFound($"Không tìm thấy gia đình với ID '{request.FamilyId}'.");
        }

        // Đảm bảo FamilyLimitConfiguration được khởi tạo
        var newConfigCreated = family.EnsureFamilyLimitConfigurationExists();
        if (newConfigCreated)
        {
            _context.FamilyLimitConfigurations.Add(family.FamilyLimitConfiguration!);
        }
        await _context.SaveChangesAsync(cancellationToken);

        // Kiểm tra hạn ngạch AI Chat
        if (family.FamilyLimitConfiguration!.AiChatMonthlyUsage >= family.FamilyLimitConfiguration.AiChatMonthlyLimit)
        {
            _logger.LogWarning("AI Chat quota exceeded for FamilyId {FamilyId}. Usage: {Usage}, Limit: {Limit}",
                request.FamilyId, family.FamilyLimitConfiguration.AiChatMonthlyUsage, family.FamilyLimitConfiguration.AiChatMonthlyLimit);
            return Result.Failure(ErrorMessages.AiChatQuotaExceeded, ErrorSources.QuotaExceeded);
        }

        family.FamilyLimitConfiguration.IncrementAiChatUsage();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AI Chat usage incremented for FamilyId {FamilyId}. New usage: {Usage}",
            request.FamilyId, family.FamilyLimitConfiguration.AiChatMonthlyUsage);

        return Result.Success();
    }
}
