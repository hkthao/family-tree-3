using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using backend.Application.UserProfiles.Specifications;
using Ardalis.Specification.EntityFrameworkCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using backend.Application.UserActivities.Commands.RecordActivity;

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMediator _mediator;

    public CreateFamilyCommandHandler(IApplicationDbContext context, IUser user, IMediator mediator)
    {
        _context = context;
        _user = user;
        _mediator = mediator;
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

            var userProfile = await _context.UserProfiles.WithSpecification(new UserProfileByAuth0UserIdSpecification(currentUserId)).FirstOrDefaultAsync(cancellationToken);
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
                Visibility = request.Visibility
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

            // Record activity
            await _mediator.Send(new RecordActivityCommand
            {
                UserProfileId = userProfile.Id,
                ActionType = UserActionType.CreateFamily,
                TargetType = TargetType.Family,
                TargetId = entity.Id,
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
}