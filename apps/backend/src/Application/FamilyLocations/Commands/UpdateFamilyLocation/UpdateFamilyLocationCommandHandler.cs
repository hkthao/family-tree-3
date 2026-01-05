using backend.Application.Common.Constants; // ADDED for ErrorMessages and ErrorSources
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events;

namespace backend.Application.FamilyLocations.Commands.UpdateFamilyLocation;

public class UpdateFamilyLocationCommandHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser, IAuthorizationService authorizationService) : IRequestHandler<UpdateFamilyLocationCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result> Handle(UpdateFamilyLocationCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        var familyLocation = await _context.FamilyLocations
                                           .Include(fl => fl.Location) // Ensure Location is loaded
                                           .FirstOrDefaultAsync(fl => fl.Id == request.Id, cancellationToken);

        if (familyLocation == null)
        {
            return Result.NotFound($"FamilyLocation with id {request.Id} not found.");
        }

        // Check if the LocationId matches the one associated with the FamilyLocation
        if (familyLocation.LocationId != request.LocationId)
        {
            return Result.Failure($"LocationId {request.LocationId} does not match the location associated with FamilyLocation {request.Id}.", ErrorSources.BadRequest);
        }

        // 2. Authorization: Check if user can access the family that the location belongs to
        if (!_authorizationService.CanAccessFamily(familyLocation.FamilyId))
        {
            return Result.Forbidden(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // FamilyId cannot be changed through this command as it's a private set.
        // If a FamilyLocation needs to be moved to another Family, a separate domain operation would be required.

        // Update Location properties
        familyLocation.Location.Update(
            request.LocationName,
            request.LocationDescription,
            request.LocationLatitude,
            request.LocationLongitude,
            request.LocationAddress,
            request.LocationType,
            request.LocationAccuracy,
            request.LocationSource
        );

        _context.FamilyLocations.Update(familyLocation); // Explicitly mark as modified to ensure change tracking.
        familyLocation.AddDomainEvent(new FamilyLocationUpdatedEvent(familyLocation));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
