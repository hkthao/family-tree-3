using Ardalis.Specification.EntityFrameworkCore; // Added for WithSpecification
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications; // Added for FamilyByIdWithMembersAndRelationshipsSpecification

namespace backend.Application.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser, IDateTime dateTime) : IRequestHandler<DeleteMemberCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _context.Members.FindAsync(request.Id);
            if (member == null)
            {
                return Result.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.Id}"), ErrorSources.NotFound);
            }

            if (!_authorizationService.CanManageFamily(member.FamilyId))
            {
                return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }

            var family = await _context.Families
                .WithSpecification(new FamilyByIdWithMembersAndRelationshipsSpecification(member.FamilyId))
                .FirstOrDefaultAsync(cancellationToken);

            if (family == null)
            {
                return Result.Failure(string.Format(ErrorMessages.NotFound, $"Family with ID {member.FamilyId}"), ErrorSources.NotFound);
            }

            family.RemoveMember(request.Id);
            _context.Members.Remove(member);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
        }
    }
}
