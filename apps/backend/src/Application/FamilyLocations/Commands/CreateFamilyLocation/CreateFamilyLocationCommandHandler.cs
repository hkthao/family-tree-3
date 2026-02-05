using backend.Application.Common.Constants; // ADDED for ErrorMessages and ErrorSources
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Domain.Entities; // Needed for FamilyLocation entity
using backend.Domain.Events; // Needed for FamilyLocationCreatedEvent

namespace backend.Application.FamilyLocations.Commands.CreateFamilyLocation;

public class CreateFamilyLocationCommandHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser, IAuthorizationService authorizationService) : IRequestHandler<CreateFamilyLocationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<Guid>> Handle(CreateFamilyLocationCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (!_currentUser.IsAuthenticated)
        {
            return Result<Guid>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        // 2. Check if the FamilyId exists
        var familyExists = await _context.Families.AnyAsync(f => f.Id == request.FamilyId, cancellationToken);
        if (!familyExists)
        {
            return Result<Guid>.NotFound(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        // 3. Authorization: Check if user can access the family
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<Guid>.Forbidden(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Create Location entity
        var location = new Location(
            request.LocationName,
            request.LocationDescription,
            request.LocationLatitude,
            request.LocationLongitude,
            request.LocationAddress,
            request.LocationType,
            request.LocationAccuracy,
            request.LocationSource
        );

        _context.Locations.Add(location);

        // Create FamilyLocation entity
        var familyLocation = new FamilyLocation(request.FamilyId, location.Id);

        familyLocation.AddDomainEvent(new FamilyLocationCreatedEvent(familyLocation));

        _context.FamilyLocations.Add(familyLocation);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(familyLocation.Id);
    }
}
