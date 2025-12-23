using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using backend.Application.Common.Constants;

namespace backend.Application.Families.Commands.UpdateFamilyLimitConfiguration;

/// <summary>
/// Handler cho UpdateFamilyLimitConfigurationCommand.
/// </summary>
public class UpdateFamilyLimitConfigurationCommandHandler : IRequestHandler<UpdateFamilyLimitConfigurationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public UpdateFamilyLimitConfigurationCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result> Handle(UpdateFamilyLimitConfigurationCommand request, CancellationToken cancellationToken)
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

        family.UpdateFamilyConfiguration(request.MaxMembers, request.MaxStorageMb, request.AiChatMonthlyLimit);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
