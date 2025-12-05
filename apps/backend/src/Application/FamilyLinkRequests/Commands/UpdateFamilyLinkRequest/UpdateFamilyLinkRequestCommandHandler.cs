using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyLinkRequests.Commands.UpdateFamilyLinkRequest;

public class UpdateFamilyLinkRequestCommandHandler : IRequestHandler<UpdateFamilyLinkRequestCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public UpdateFamilyLinkRequestCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(UpdateFamilyLinkRequestCommand request, CancellationToken cancellationToken)
    {
        var familyLinkRequest = await _context.FamilyLinkRequests
            .FirstOrDefaultAsync(flr => flr.Id == request.Id, cancellationToken);

        if (familyLinkRequest == null)
        {
            return Result<Unit>.NotFound("Không tìm thấy yêu cầu liên kết gia đình.");
        }

        // Authorization: Only admin of target family can approve/reject
        if (request.Status == LinkStatus.Approved || request.Status == LinkStatus.Rejected)
        {
            if (!_authorizationService.CanManageFamily(familyLinkRequest.TargetFamilyId))
            {
                return Result<Unit>.Forbidden("Bạn không có quyền phê duyệt hoặc từ chối yêu cầu này.");
            }
        }
        else // Other updates can be done by either requesting or target family admin
        {
            if (!_authorizationService.CanManageFamily(familyLinkRequest.RequestingFamilyId) &&
                !_authorizationService.CanManageFamily(familyLinkRequest.TargetFamilyId))
            {
                return Result<Unit>.Forbidden("Bạn không có quyền cập nhật yêu cầu liên kết gia đình này.");
            }
        }

        familyLinkRequest.UpdateStatus(request.Status);
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
