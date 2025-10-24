using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using backend.Domain.Events.Members;

namespace backend.Application.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator, IFamilyTreeService familyTreeService) : IRequestHandler<DeleteMemberCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;

    public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
            if (currentUserProfile == null)
            {
                return Result.Failure("User profile not found.", "NotFound");
            }

            // Authorization check
            // First, get the member to find its familyId
            var memberToDelete = await _context.Members.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (memberToDelete == null)
            {
                return Result.Failure($"Member with ID {request.Id} not found.", "NotFound");
            }

            if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(memberToDelete.FamilyId, currentUserProfile))
            {
                return Result.Failure("Access denied. Only family managers can delete members.", "Forbidden");
            }

            var memberFullName = memberToDelete.FullName; // Capture full name for activity summary
            var familyId = memberToDelete.FamilyId; // Capture familyId before deletion

            memberToDelete.AddDomainEvent(new MemberDeletedEvent(memberToDelete));
            _context.Members.Remove(memberToDelete);
            await _context.SaveChangesAsync(cancellationToken);

            // Update family stats
            await _familyTreeService.UpdateFamilyStats(familyId, cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure($"Database error occurred while deleting member: {ex.Message}", "Database");
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure($"An unexpected error occurred while deleting member: {ex.Message}", "Exception");
        }
    }
}
