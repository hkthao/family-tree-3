using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyLinkRequests.Commands.RejectFamilyLinkRequest;

public class RejectFamilyLinkRequestCommandHandler : IRequestHandler<RejectFamilyLinkRequestCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public RejectFamilyLinkRequestCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(RejectFamilyLinkRequestCommand request, CancellationToken cancellationToken)
    {
        var linkRequest = await _context.FamilyLinkRequests
            .FirstOrDefaultAsync(lr => lr.Id == request.RequestId, cancellationToken);

        if (linkRequest == null)
        {
            return Result<Unit>.NotFound("Không tìm thấy yêu cầu liên kết gia đình.");
        }

        // 1. Authorization: Only admin of the TargetFamilyId can reject the request
        if (!_authorizationService.CanManageFamily(linkRequest.TargetFamilyId))
        {
            return Result<Unit>.Forbidden("Bạn không có quyền từ chối yêu cầu liên kết này.");
        }

        // 2. Validation: Request must be pending
        if (linkRequest.Status != LinkStatus.Pending)
        {
            return Result<Unit>.Conflict("Yêu cầu liên kết không ở trạng thái chờ xử lý.");
        }

        // 3. Reject the request
        linkRequest.Reject(request.ResponseMessage);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
