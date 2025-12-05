using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.FamilyLinkRequests.Commands.ApproveFamilyLinkRequest;

public class ApproveFamilyLinkRequestCommandHandler : IRequestHandler<ApproveFamilyLinkRequestCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public ApproveFamilyLinkRequestCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(ApproveFamilyLinkRequestCommand request, CancellationToken cancellationToken)
    {
        var linkRequest = await _context.FamilyLinkRequests
            .Include(lr => lr.RequestingFamily)
            .Include(lr => lr.TargetFamily)
            .FirstOrDefaultAsync(lr => lr.Id == request.RequestId, cancellationToken);

        if (linkRequest == null)
        {
            return Result<Unit>.NotFound("Không tìm thấy yêu cầu liên kết gia đình.");
        }

        // 1. Authorization: Only admin of the TargetFamilyId can approve the request
        if (!_authorizationService.CanManageFamily(linkRequest.TargetFamilyId))
        {
            return Result<Unit>.Forbidden("Bạn không có quyền phê duyệt yêu cầu liên kết này.");
        }

        // 2. Validation: Request must be pending
        if (linkRequest.Status != LinkStatus.Pending)
        {
            return Result<Unit>.Conflict("Yêu cầu liên kết không ở trạng thái chờ xử lý.");
        }

        // 3. Check for existing active link (A-B or B-A) - should not happen if previous checks are robust, but good for safety
        var existingActiveLink = await _context.FamilyLinks
            .AnyAsync(fl => (fl.Family1Id == linkRequest.RequestingFamilyId && fl.Family2Id == linkRequest.TargetFamilyId) ||
                            (fl.Family1Id == linkRequest.TargetFamilyId && fl.Family2Id == linkRequest.RequestingFamilyId),
                            cancellationToken);

        if (existingActiveLink)
        {
            // Update the request to Approved even if a link already exists to reflect the admin's intent
            linkRequest.Approve(request.ResponseMessage);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Conflict("Hai gia đình này đã được liên kết.");
        }

        // 4. Approve the request
        linkRequest.Approve(request.ResponseMessage);

        // 5. Create a new FamilyLink
        var familyLink = new FamilyLink(linkRequest.RequestingFamilyId, linkRequest.TargetFamilyId);
        _context.FamilyLinks.Add(familyLink);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}