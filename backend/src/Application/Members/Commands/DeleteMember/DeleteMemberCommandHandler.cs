using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;

namespace backend.Application.Members.Commands.DeleteMember
{
    public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMediator _mediator;
        private readonly IFamilyTreeService _familyTreeService;

        public DeleteMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator, IFamilyTreeService familyTreeService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _mediator = mediator;
            _familyTreeService = familyTreeService;
        }

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
                var memberToDelete = await _context.Members.FindAsync(new object[] { request.Id }, cancellationToken);
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

                _context.Members.Remove(memberToDelete);
                await _context.SaveChangesAsync(cancellationToken);

                // Update family stats
                await _familyTreeService.UpdateFamilyStats(familyId, cancellationToken);

                // Record activity
                await _mediator.Send(new RecordActivityCommand
                {
                    UserProfileId = currentUserProfile.Id,
                    ActionType = UserActionType.DeleteMember,
                    TargetType = TargetType.Member,
                    TargetId = request.Id.ToString(),
                    ActivitySummary = $"Deleted member '{memberFullName}' from family '{familyId}'."
                }, cancellationToken);

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
}
