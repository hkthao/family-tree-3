using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Families.Queries.GetPrivacyConfiguration;

public class GetPrivacyConfigurationQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService) : IRequestHandler<GetPrivacyConfigurationQuery, Result<PrivacyConfigurationDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<PrivacyConfigurationDto>> Handle(GetPrivacyConfigurationQuery request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<PrivacyConfigurationDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var privacyConfig = await _context.PrivacyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.FamilyId == request.FamilyId, cancellationToken);

        if (privacyConfig == null)
        {
            // If no explicit configuration exists, return a default (e.g., all properties public)
            // Or create a default one and return it
            privacyConfig = new PrivacyConfiguration(request.FamilyId);
            // Optionally, save this default config to the database
            // _context.PrivacyConfigurations.Add(privacyConfig);
            // await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<PrivacyConfigurationDto>.Success(_mapper.Map<PrivacyConfigurationDto>(privacyConfig));
    }
}
