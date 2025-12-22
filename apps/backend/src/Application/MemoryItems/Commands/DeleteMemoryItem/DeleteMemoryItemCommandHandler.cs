using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.MemoryItems.Commands.DeleteMemoryItem;

public class DeleteMemoryItemCommandHandler : IRequestHandler<DeleteMemoryItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IAuthorizationService _authorizationService; // Inject IAuthorizationService

    public DeleteMemoryItemCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IDateTime dateTime, IAuthorizationService authorizationService)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _authorizationService = authorizationService;
    }

    public async Task<Result> Handle(DeleteMemoryItemCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        var entity = await _context.MemoryItems
            .FirstOrDefaultAsync(mi => mi.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result.NotFound();
        }

        // Authorization: Check if user can access the family
        if (!_authorizationService.CanAccessFamily(entity.FamilyId))
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedDate = _dateTime.Now;
        entity.DeletedBy = _currentUser.UserId.ToString();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
