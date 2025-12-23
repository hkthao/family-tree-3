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

        var entity = await _context.FamilyLocations.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return Result.NotFound($"FamilyLocation with id {request.Id} not found.");
        }

        // 2. Authorization: Check if user can access the family that the location belongs to
        if (!_authorizationService.CanAccessFamily(entity.FamilyId))
        {
            return Result.Forbidden(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 3. Optional: If FamilyId is being changed, ensure user has access to the *new* family as well.
        // For simplicity, we assume FamilyId cannot be changed through this command or handled externally.
        // If it can be changed, a more robust check here would be needed.
        if (request.FamilyId != entity.FamilyId)
        {
            // If family ID is being updated, verify access to the new family as well
            if (!_authorizationService.CanAccessFamily(request.FamilyId))
            {
                return Result.Forbidden(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }
        }

        _mapper.Map(request, entity);
        entity.AddDomainEvent(new FamilyLocationUpdatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
