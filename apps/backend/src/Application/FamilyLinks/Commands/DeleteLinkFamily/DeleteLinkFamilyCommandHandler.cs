using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Commands.DeleteLinkFamily;

public class DeleteLinkFamilyCommandHandler : IRequestHandler<DeleteLinkFamilyCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public DeleteLinkFamilyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(DeleteLinkFamilyCommand request, CancellationToken cancellationToken)
    {
        var familyLink = await _context.FamilyLinks
            .FirstOrDefaultAsync(fl => fl.Id == request.FamilyLinkId, cancellationToken);

        if (familyLink == null)
        {
            return Result<Unit>.NotFound("Không tìm thấy liên kết gia đình.");
        }

        // 1. Authorization: Only admin of either family can unlink
        if (!_authorizationService.CanManageFamily(familyLink.Family1Id) && !_authorizationService.CanManageFamily(familyLink.Family2Id))
        {
            return Result<Unit>.Forbidden("Bạn không có quyền hủy liên kết giữa các gia đình này.");
        }

        // 2. Remove the FamilyLink entity
        _context.FamilyLinks.Remove(familyLink);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
