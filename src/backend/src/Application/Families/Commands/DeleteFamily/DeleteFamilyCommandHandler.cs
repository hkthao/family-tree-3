using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.Events;
using backend.Domain.Events.Families;

namespace backend.Application.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IUser user) : IRequestHandler<DeleteFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IUser _user = user;

    public async Task<Result> Handle(DeleteFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_user.Id.HasValue)
            {
                return Result.Failure("User is not authenticated.", "Authentication");
            }

            if (!_authorizationService.CanManageFamily(request.Id))
            {
                return Result.Failure("User does not have permission to delete this family.", "Forbidden");
            }

            var entity = await _context.Families.WithSpecification(new FamilyByIdSpecification(request.Id)).FirstOrDefaultAsync(cancellationToken);
            if (entity == null)
                return Result.Failure($"Family with ID {request.Id} not found.", "NotFound");

            entity.AddDomainEvent(new FamilyDeletedEvent(entity));
            _context.Families.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            // Update family stats
            entity.AddDomainEvent(new FamilyStatsUpdatedEvent(request.Id));

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
