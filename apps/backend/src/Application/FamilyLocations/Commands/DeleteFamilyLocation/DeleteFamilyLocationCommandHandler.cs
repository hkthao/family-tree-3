using backend.Application.Common.Constants; // ADDED for ErrorMessages and ErrorSources
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events;

namespace backend.Application.FamilyLocations.Commands.DeleteFamilyLocation;

public class DeleteFamilyLocationCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IAuthorizationService authorizationService) : IRequestHandler<DeleteFamilyLocationCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result> Handle(DeleteFamilyLocationCommand request, CancellationToken cancellationToken)
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

        _context.FamilyLocations.Remove(entity);
        entity.AddDomainEvent(new FamilyLocationDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
