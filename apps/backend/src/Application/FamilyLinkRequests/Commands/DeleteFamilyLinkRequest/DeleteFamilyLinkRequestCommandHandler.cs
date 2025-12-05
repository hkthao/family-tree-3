using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.FamilyLinkRequests.Commands.DeleteFamilyLinkRequest;

public class DeleteFamilyLinkRequestCommandHandler : IRequestHandler<DeleteFamilyLinkRequestCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public DeleteFamilyLinkRequestCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(DeleteFamilyLinkRequestCommand request, CancellationToken cancellationToken)
    {
        var familyLinkRequest = await _context.FamilyLinkRequests
            .FirstOrDefaultAsync(flr => flr.Id == request.Id, cancellationToken);

        if (familyLinkRequest == null)
        {
            return Result<Unit>.NotFound("Không tìm thấy yêu cầu liên kết gia đình.");
        }

        // Authorization: Only admin of requesting or target family can delete
        if (!_authorizationService.CanManageFamily(familyLinkRequest.RequestingFamilyId) &&
            !_authorizationService.CanManageFamily(familyLinkRequest.TargetFamilyId))
        {
            return Result<Unit>.Forbidden("Bạn không có quyền xóa yêu cầu liên kết gia đình này.");
        }

        _context.FamilyLinkRequests.Remove(familyLinkRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
