using backend.Application.Common.Constants; // ADDED for ErrorMessages and ErrorSources
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyLocations.Queries.GetFamilyLocationById;

public record GetFamilyLocationByIdQuery(Guid Id) : IRequest<Result<FamilyLocationDto>>;

public class GetFamilyLocationByIdQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser, IAuthorizationService authorizationService, IPrivacyService privacyService) : IRequestHandler<GetFamilyLocationByIdQuery, Result<FamilyLocationDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<FamilyLocationDto>> Handle(GetFamilyLocationByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (!_currentUser.IsAuthenticated)
        {
            return Result<FamilyLocationDto>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        var entity = await _context.FamilyLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<FamilyLocationDto>.NotFound($"FamilyLocation with id {request.Id} not found.");
        }

        // 2. Authorization: Check if user can access the family that the location belongs to
        if (!_authorizationService.CanAccessFamily(entity.FamilyId))
        {
            // Return NotFound to avoid leaking information about the existence of the family location
            return Result<FamilyLocationDto>.NotFound();
        }

        var familyLocationDto = _mapper.Map<FamilyLocationDto>(entity);

        // Apply privacy filter
        var filteredFamilyLocationDto = await _privacyService.ApplyPrivacyFilter(familyLocationDto, entity.FamilyId, cancellationToken);

        return Result<FamilyLocationDto>.Success(filteredFamilyLocationDto);
    }
}
