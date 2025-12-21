using backend.Application.Common.Interfaces; // ADDED
using backend.Application.Common.Models;

namespace backend.Application.Families.Commands;

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

/// <summary>
/// Handler cho ResetFamilyAiChatQuotaCommand.
/// </summary>
public class ResetFamilyAiChatQuotaCommandHandler : IRequestHandler<ResetFamilyAiChatQuotaCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ResetFamilyAiChatQuotaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ResetFamilyAiChatQuotaCommand request, CancellationToken cancellationToken)
    {
        var family = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result.NotFound($"Không tìm thấy gia đình với ID '{request.FamilyId}'.");
        }

        if (family.FamilyLimitConfiguration == null)
        {
            return Result.NotFound($"Không tìm thấy cấu hình giới hạn cho gia đình với ID '{request.FamilyId}'.");
        }

        family.FamilyLimitConfiguration.ResetAiChatUsage();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
