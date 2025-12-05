using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;

namespace backend.Application.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser) : IRequestHandler<GetFamilyByIdQuery, Result<FamilyDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<FamilyDetailDto>> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        // Handle unauthenticated user first, return failure (or unauthorized)
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
        {
            return Result<FamilyDetailDto>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        var query = _context.Families.AsQueryable();

        // Apply FamilyAccessSpecification to filter families based on user's access
        query = query.WithSpecification(new FamilyAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));
        query = query.WithSpecification(new FamilyByIdSpecification(request.Id)); // Apply the FamilyByIdSpecification after access control

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var familyDto = await query
            .ProjectTo<FamilyDetailDto>(_mapper.ConfigurationProvider)
            .AsSplitQuery() // Keep AsSplitQuery if it's still needed, moved to after specifications
            .FirstOrDefaultAsync(cancellationToken);

        return familyDto == null
            ? Result<FamilyDetailDto>.Failure(string.Format(ErrorMessages.FamilyNotFound, request.Id), ErrorSources.NotFound)
            : Result<FamilyDetailDto>.Success(familyDto);
    }
}
