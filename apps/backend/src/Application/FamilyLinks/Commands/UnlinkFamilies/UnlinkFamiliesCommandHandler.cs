using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Commands.UnlinkFamilies;

public class UnlinkFamiliesCommandHandler : IRequestHandler<UnlinkFamiliesCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public UnlinkFamiliesCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(UnlinkFamiliesCommand request, CancellationToken cancellationToken)
    {
        var familyLink = await _context.FamilyLinks
            .FirstOrDefaultAsync(fl => (fl.Family1Id == request.Family1Id && fl.Family2Id == request.Family2Id) ||
                                       (fl.Family1Id == request.Family2Id && fl.Family2Id == request.Family1Id),
                                       cancellationToken);

        if (familyLink == null)
        {
            return Result<Unit>.NotFound("Không tìm thấy liên kết gia đình.");
        }

        // 1. Authorization: Only admin of either family can unlink
        if (!_authorizationService.CanManageFamily(request.Family1Id) && !_authorizationService.CanManageFamily(request.Family2Id))
        {
            return Result<Unit>.Forbidden("Bạn không có quyền hủy liên kết giữa các gia đình này.");
        }

        // 2. Remove the FamilyLink entity
        _context.FamilyLinks.Remove(familyLink);

        // 3. Update the corresponding FamilyLinkRequest to Rejected status
        var linkRequest = await _context.FamilyLinkRequests
            .FirstOrDefaultAsync(lr => (lr.RequestingFamilyId == request.Family1Id && lr.TargetFamilyId == request.Family2Id) ||
                                       (lr.RequestingFamilyId == request.Family2Id && lr.TargetFamilyId == request.Family1Id),
                                       cancellationToken);

        if (linkRequest != null)
        {
            linkRequest.MarkAsPending(); // Mark as pending so a new request can be sent.
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
