using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Families.Commands;

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

/// <summary>
/// Handler cho UpdateFamilyLimitConfigurationCommand.
/// </summary>
public class UpdateFamilyLimitConfigurationCommandHandler : IRequestHandler<UpdateFamilyLimitConfigurationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateFamilyLimitConfigurationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateFamilyLimitConfigurationCommand request, CancellationToken cancellationToken)
    {
        var family = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result.NotFound($"Không tìm thấy gia đình với ID '{request.FamilyId}'.");
        }

        family.UpdateFamilyConfiguration(request.MaxMembers, request.MaxStorageMb, request.AiChatMonthlyLimit);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
