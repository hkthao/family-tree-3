using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Families.PrivacyConfigurations.Commands;

public class UpdatePrivacyConfigurationCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<UpdatePrivacyConfigurationCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<Unit>> Handle(UpdatePrivacyConfigurationCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanManageFamily(request.FamilyId))
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var privacyConfig = await _context.PrivacyConfigurations
            .FirstOrDefaultAsync(pc => pc.FamilyId == request.FamilyId, cancellationToken);

        if (privacyConfig == null)
        {
            privacyConfig = new PrivacyConfiguration(request.FamilyId);
            _context.PrivacyConfigurations.Add(privacyConfig);
        }

        privacyConfig.UpdatePublicMemberProperties(request.PublicMemberProperties);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
