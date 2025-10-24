using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.Enums;
using backend.Domain.Events.Families;

namespace backend.Application.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator, IFamilyTreeService familyTreeService) : IRequestHandler<DeleteFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;

    public async Task<Result> Handle(DeleteFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
            if (currentUserProfile == null)
            {
                return Result.Failure("User profile not found.", "NotFound");
            }
            if (!_authorizationService.IsAdmin())
            {
                if (!_authorizationService.CanManageFamily(request.Id, currentUserProfile))
                {
                    return Result.Failure("User does not have permission to delete this family.", "Forbidden");
                }
            }

            var entity = await _context.Families.WithSpecification(new FamilyByIdSpecification(request.Id)).FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                return Result.Failure($"Family with ID {request.Id} not found.", "NotFound");
            }

            var familyName = entity.Name; // Capture family name for activity summary

            entity.AddDomainEvent(new FamilyDeletedEvent(entity));
            _context.Families.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            // Update family stats
            await _familyTreeService.UpdateFamilyStats(request.Id, cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure($"Database error occurred while deleting family: {ex.Message}", "Database");
        }
        catch (Exception ex)
        {
            return Result.Failure($"An unexpected error occurred while deleting family: {ex.Message}", "Exception");
        }
    }
}
