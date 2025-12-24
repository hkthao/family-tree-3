using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Constants;

namespace backend.Application.Families.Commands.ResetFamilyAiChatQuota;

/// <summary>
/// Handler để xử lý lệnh ResetFamilyAiChatQuotaCommand.
/// </summary>
public class ResetFamilyAiChatQuotaCommandHandler : IRequestHandler<ResetFamilyAiChatQuotaCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public ResetFamilyAiChatQuotaCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result> Handle(ResetFamilyAiChatQuotaCommand request, CancellationToken cancellationToken)
    {
        var authorized = await _authorizationService.AuthorizeAsync(AppRoles.Administrator);
        if (!authorized.IsSuccess)
        {
            return Result.Unauthorized(authorized.Error ?? "Lỗi ủy quyền không xác định.");
        }

        var family = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result.NotFound($"Không tìm thấy gia đình với ID '{request.FamilyId}'.");
        }

        if (family.FamilyLimitConfiguration == null)
        {
            return Result.NotFound($"Gia đình với ID '{request.FamilyId}' không có cấu hình giới hạn.");
        }

        family.FamilyLimitConfiguration.ResetAiChatUsage();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
