using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using Ardalis.Specification.EntityFrameworkCore;

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandler : IRequestHandler<UpdateFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public UpdateFamilyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
            if (currentUserProfile == null)
            {
                return Result.Failure("User profile not found.", "NotFound");
            }

            if (!_authorizationService.CanManageFamily(request.Id, currentUserProfile))
            {
                return Result.Failure("User does not have permission to update this family.", "Forbidden");
            }

            var entity = await _context.Families.WithSpecification(new FamilyByIdSpecification(request.Id)).FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                return Result.Failure($"Family with ID {request.Id} not found.", "NotFound");
            }

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.Address = request.Address;
            entity.AvatarUrl = request.AvatarUrl;
            entity.Visibility = request.Visibility;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure($"Database error occurred while updating family: {ex.Message}", "Database");
        }
        catch (Exception ex)
        {
            return Result.Failure($"An unexpected error occurred while updating family: {ex.Message}", "Exception");
        }
    }
}