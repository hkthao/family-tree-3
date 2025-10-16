using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Application.UserProfiles.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMediator _mediator;
    private readonly IFamilyTreeService _familyTreeService;

    public CreateFamilyCommandHandler(IApplicationDbContext context, IUser user, IMediator mediator, IFamilyTreeService familyTreeService)
    {
        _context = context;
        _user = user;
        _mediator = mediator;
        _familyTreeService = familyTreeService;
    }

    public async Task<Result<Guid>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _user.Id;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Result<Guid>.Failure("Current user ID not found.", "Authentication");
            }

            var userProfile = await _context.UserProfiles.WithSpecification(new UserProfileByExternalIdSpecification(currentUserId)).FirstOrDefaultAsync(cancellationToken);
            if (userProfile == null)
            {
                return Result<Guid>.Failure("User profile not found.", "NotFound");
            }

            var entity = new Family
            {
                Name = request.Name,
                Description = request.Description,
                Address = request.Address,
                AvatarUrl = request.AvatarUrl,
                Visibility = request.Visibility,
                Code = request.Code ?? GenerateUniqueCode("FAM")
            };

            _context.Families.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            // Assign the creating user as a Manager of the new family
            var familyUser = new FamilyUser
            {
                FamilyId = entity.Id,
                UserProfileId = userProfile.Id,
                Role = FamilyRole.Manager
            };

            _context.FamilyUsers.Add(familyUser);
            await _context.SaveChangesAsync(cancellationToken);

            // Update family stats
            await _familyTreeService.UpdateFamilyStats(entity.Id, cancellationToken);

            // Record activity
            await _mediator.Send(new RecordActivityCommand
            {
                UserProfileId = userProfile.Id,
                ActionType = UserActionType.CreateFamily,
                TargetType = TargetType.Family,
                TargetId = entity.Id.ToString(),
                ActivitySummary = $"Created family '{entity.Name}'."
            }, cancellationToken);

            return Result<Guid>.Success(entity.Id);
        }
        catch (DbUpdateException ex)
        {
            // Log the exception details here if a logger is available
            return Result<Guid>.Failure($"Database error occurred while creating family: {ex.Message}", "Database");
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result<Guid>.Failure($"An unexpected error occurred while creating family: {ex.Message}", "Exception");
        }
    }

    private string GenerateUniqueCode(string prefix)
    {
        // For simplicity, generate a GUID and take a substring.
        // In a real application, you'd want to ensure uniqueness against existing codes in the database.
        return $"{prefix}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}";
    }
}
