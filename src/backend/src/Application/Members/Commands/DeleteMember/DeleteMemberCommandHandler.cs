using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.Members;
using backend.Domain.Events.Families;

namespace backend.Application.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IUser currentUser, IDateTime dateTime) : IRequestHandler<DeleteMemberCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var memberToDelete = await _context.Members.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (memberToDelete == null)
            {
                return Result.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.Id}"), ErrorSources.NotFound);
            }

            if (!_authorizationService.CanManageFamily(memberToDelete.FamilyId))
            {
                return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }

            var memberFullName = memberToDelete.FullName; // Capture full name for activity summary
            var familyId = memberToDelete.FamilyId; // Capture familyId before deletion

            memberToDelete.IsDeleted = true;
            memberToDelete.DeletedBy = _currentUser.Id?.ToString();
            memberToDelete.DeletedDate = _dateTime.Now;

            memberToDelete.AddDomainEvent(new MemberDeletedEvent(memberToDelete));
            await _context.SaveChangesAsync(cancellationToken);

            // Update family stats via domain event
            memberToDelete.AddDomainEvent(new FamilyStatsUpdatedEvent(familyId));

            return Result.Success();
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
        }
    }
}
