using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser, IPrivacyService privacyService) : IRequestHandler<GetFamiliesByIdsQuery, Result<List<FamilyDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<List<FamilyDto>>> Handle(GetFamiliesByIdsQuery request, CancellationToken cancellationToken)
    {
        // Handle unauthenticated user first, return empty list
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
        {
            return Result<List<FamilyDto>>.Success(new List<FamilyDto>());
        }

        var query = _context.Families.AsQueryable();

        // Apply FamilyAccessSpecification to filter families based on user's access
        query = query.WithSpecification(new FamilyAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));
        query = query.WithSpecification(new FamiliesByIdsSpecification(request.Ids));

        var familyEntities = await query
            .ToListAsync(cancellationToken);

        var familyDtos = _mapper.Map<List<FamilyDto>>(familyEntities);

        var filteredFamilyDtos = new List<FamilyDto>();
        foreach (var familyDto in familyDtos)
        {
            // Apply privacy filter to each FamilyDto
            filteredFamilyDtos.Add(await _privacyService.ApplyPrivacyFilter(familyDto, familyDto.Id, cancellationToken));
        }

        return Result<List<FamilyDto>>.Success(filteredFamilyDtos);
    }
}
