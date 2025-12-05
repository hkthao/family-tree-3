using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.FamilyLinkRequests.Commands.CreateFamilyLinkRequest;

public class CreateFamilyLinkRequestCommandHandler : IRequestHandler<CreateFamilyLinkRequestCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService; // For family admin check
    private readonly ICurrentUser _currentUser; // For current user ID

    public CreateFamilyLinkRequestCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateFamilyLinkRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Authorization: Only admin of RequestingFamilyId can send requests
        if (!_authorizationService.CanManageFamily(request.RequestingFamilyId))
        {
            return Result<Guid>.Forbidden("Bạn không có quyền gửi yêu cầu liên kết cho gia đình này.");
        }

        // 2. Validation: Ensure both families exist
        var requestingFamily = await _context.Families.FindAsync(new object[] { request.RequestingFamilyId }, cancellationToken);
        var targetFamily = await _context.Families.FindAsync(new object[] { request.TargetFamilyId }, cancellationToken);

        if (requestingFamily == null)
        {
            return Result<Guid>.NotFound($"Không tìm thấy gia đình gửi yêu cầu (ID: {request.RequestingFamilyId}).");
        }
        if (targetFamily == null)
        {
            return Result<Guid>.NotFound($"Không tìm thấy gia đình nhận yêu cầu (ID: {request.TargetFamilyId}).");
        }

        // 3. Validation: Cannot link a family to itself
        if (request.RequestingFamilyId == request.TargetFamilyId)
        {
            return Result<Guid>.Conflict("Không thể gửi yêu cầu liên kết đến chính gia đình của bạn.");
        }

        // 4. Validation: Check for existing requests (Pending, Approved, Rejected) or active links
        var existingRequest = await _context.FamilyLinkRequests
            .Where(r => (r.RequestingFamilyId == request.RequestingFamilyId && r.TargetFamilyId == request.TargetFamilyId) ||
                        (r.RequestingFamilyId == request.TargetFamilyId && r.TargetFamilyId == request.RequestingFamilyId))
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRequest != null)
        {
            if (existingRequest.Status == Domain.Enums.LinkStatus.Pending)
            {
                return Result<Guid>.Conflict("Đã có yêu cầu liên kết đang chờ xử lý giữa hai gia đình này.");
            }
            if (existingRequest.Status == Domain.Enums.LinkStatus.Approved)
            {
                return Result<Guid>.Conflict("Hai gia đình này đã được liên kết.");
            }
            // If rejected, allow new request
        }

        // Check for active link (should be covered by approved request, but good to double check)
        var existingLink = await _context.FamilyLinks
            .Where(l => (l.Family1Id == request.RequestingFamilyId && l.Family2Id == request.TargetFamilyId) ||
                        (l.Family1Id == request.TargetFamilyId && l.Family2Id == request.RequestingFamilyId))
            .FirstOrDefaultAsync(cancellationToken);

        if (existingLink != null)
        {
            return Result<Guid>.Conflict("Hai gia đình này đã được liên kết.");
        }


        // 5. Create new request
        var familyLinkRequest = new FamilyLinkRequest(request.RequestingFamilyId, request.TargetFamilyId, request.RequestMessage);
        _context.FamilyLinkRequests.Add(familyLinkRequest);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(familyLinkRequest.Id);
    }
}