using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser) : IRequestHandler<GetFamiliesByIdsQuery, Result<List<FamilyDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;

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


        var familyList = await query
            .ProjectTo<FamilyDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<FamilyDto>>.Success(familyList);
    }
}