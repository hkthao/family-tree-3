using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.Commands.EnsureFamilyAiConfigExists;

/// <summary>
/// Xử lý lệnh để đảm bảo cấu hình giới hạn AI cho gia đình tồn tại.
/// Nếu không tồn tại, nó sẽ được tạo với các giá trị mặc định.
/// </summary>
public class EnsureFamilyAiConfigExistsCommandHandler : IRequestHandler<EnsureFamilyAiConfigExistsCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EnsureFamilyAiConfigExistsCommandHandler> _logger;

    public EnsureFamilyAiConfigExistsCommandHandler(IApplicationDbContext context, ILogger<EnsureFamilyAiConfigExistsCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(EnsureFamilyAiConfigExistsCommand request, CancellationToken cancellationToken)
    {
        var family = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            _logger.LogWarning("Không tìm thấy gia đình với ID: {FamilyId}.", request.FamilyId);
            return Result.NotFound(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        var newConfigCreated = family.EnsureFamilyLimitConfigurationExists();
        if (newConfigCreated)
        {
            _context.FamilyLimitConfigurations.Add(family.FamilyLimitConfiguration!); // FamilyLimitConfiguration will not be null here
            _logger.LogInformation("Đã tạo cấu hình giới hạn AI mới cho gia đình: {FamilyId}.", request.FamilyId);
        }
        else
        {
            _logger.LogInformation("Cấu hình giới hạn AI đã tồn tại cho gia đình: {FamilyId}.", request.FamilyId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
