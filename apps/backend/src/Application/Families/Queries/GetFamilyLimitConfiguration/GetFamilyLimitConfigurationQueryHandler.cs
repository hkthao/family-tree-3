using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Families.Queries;

/// <summary>
/// Handler cho GetFamilyLimitConfigurationQuery.
/// </summary>
public class GetFamilyLimitConfigurationQueryHandler : IRequestHandler<GetFamilyLimitConfigurationQuery, Result<FamilyLimitConfigurationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyLimitConfigurationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<FamilyLimitConfigurationDto>> Handle(GetFamilyLimitConfigurationQuery request, CancellationToken cancellationToken)
    {
        FamilyLimitConfigurationDto? familyLimitConfiguration = await _context.FamilyLimitConfigurations
            .Where(f => f.FamilyId == request.FamilyId)
            .ProjectTo<FamilyLimitConfigurationDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        // If not found, return a default/empty DTO or throw an exception based on business rules
        // For now, let's return a default DTO if no specific configuration is found.
        // This assumes that if a Family exists, a FamilyLimitConfiguration should also exist (created with default values).
        // If not, it means there's an inconsistency or the Family was created before this feature.
        // In a real-world scenario, you might want to handle this more robustly.
        if (familyLimitConfiguration == null)
        {
            // Optionally, create a new one with default values if not found,
            // or return null and handle it in the calling code.
            // For now, assume it should always exist if the family exists.
            // If it truly doesn't exist, this might indicate a data integrity issue
            // because a Family.Create should also create a default FamilyLimitConfiguration.
            // Let's create a new DTO with default values for demonstration purposes,
            // but in a production app, you might fetch default values from a service or throw.
            return Result<FamilyLimitConfigurationDto>.Success(new FamilyLimitConfigurationDto
            {
                Id = Guid.Empty, // Placeholder for non-existent ID
                FamilyId = request.FamilyId,
                MaxMembers = 5000, // Default value
                MaxStorageMb = 2048, // Default value
                AiChatMonthlyLimit = 100 // Default value
            });
        }

        return Result<FamilyLimitConfigurationDto>.Success(familyLimitConfiguration);
    }
}
