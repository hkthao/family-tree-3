using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.Members;

namespace backend.Application.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IFamilyTreeService familyTreeService) : IRequestHandler<DeleteMemberCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;

    public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var memberToDelete = await _context.Members.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (memberToDelete == null)
            {
                return Result.Failure($"Member with ID {request.Id} not found.", "NotFound");
            }

            if (!_authorizationService.CanManageFamily(memberToDelete.FamilyId))
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
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure($"An unexpected error occurred while deleting member: {ex.Message}", "Exception");
        }
    }
}
