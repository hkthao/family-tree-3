using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Families.Commands.UpdatePrivacyConfiguration;

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
        privacyConfig.UpdatePublicEventProperties(request.PublicEventProperties);
        privacyConfig.UpdatePublicFamilyProperties(request.PublicFamilyProperties);
        privacyConfig.UpdatePublicFamilyLocationProperties(request.PublicFamilyLocationProperties);
        privacyConfig.UpdatePublicMemoryItemProperties(request.PublicMemoryItemProperties);
        privacyConfig.UpdatePublicMemberFaceProperties(request.PublicMemberFaceProperties);
        privacyConfig.UpdatePublicFoundFaceProperties(request.PublicFoundFaceProperties);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
