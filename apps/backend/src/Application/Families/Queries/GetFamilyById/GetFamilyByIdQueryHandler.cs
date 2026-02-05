using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser, IPrivacyService privacyService) : IRequestHandler<GetFamilyByIdQuery, Result<FamilyDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<FamilyDetailDto>> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        // Handle unauthenticated user first, return failure (or unauthorized)
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
        {
            return Result<FamilyDetailDto>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        var authorizedQuery = _context.Families.AsQueryable();

        // Apply FamilyAccessSpecification to filter families based on user's access
        // authorizedQuery = authorizedQuery.WithSpecification(new FamilyAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));
        authorizedQuery = authorizedQuery.WithSpecification(new FamilyByIdSpecification(request.Id)); // Apply the FamilyByIdSpecification after access control

        // Fetch the Family entity, including its limit configuration
        var family = await authorizedQuery
            .Include(f => f.FamilyLimitConfiguration) // Ensure FamilyLimitConfiguration is loaded
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            return Result<FamilyDetailDto>.Failure(string.Format(ErrorMessages.FamilyNotFound, request.Id), ErrorSources.NotFound);
        }

        // Map the Family entity to FamilyDetailDto
        var familyDetailDto = _mapper.Map<FamilyDetailDto>(family);

        // If FamilyLimitConfiguration is null, assign default values to the DTO
        if (familyDetailDto.FamilyLimitConfiguration == null)
        {
            familyDetailDto.FamilyLimitConfiguration = new FamilyLimitConfigurationDto
            {
                Id = Guid.Empty, // Placeholder for non-existent ID
                FamilyId = family.Id,
                MaxMembers = 5000, // Default value
                MaxStorageMb = 2048, // Default value
                AiChatMonthlyLimit = 100 // Default value
            };
        }

        // Apply privacy filter
        var filteredFamilyDetailDto = await _privacyService.ApplyPrivacyFilter(familyDetailDto, family.Id, cancellationToken);

        return Result<FamilyDetailDto>.Success(filteredFamilyDetailDto);
    }
}
