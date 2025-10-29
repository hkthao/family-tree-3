using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Families;

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler(IApplicationDbContext context, IUser user) : IRequestHandler<CreateFamilyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;

    public async Task<Result<Guid>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
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
            entity.AddDomainEvent(new FamilyCreatedEvent(entity));

            await _context.SaveChangesAsync(cancellationToken);

            // Assign the creating user as a Manager of the new family
            var familyUser = new FamilyUser
            {
                FamilyId = entity.Id,
                UserProfileId = _user.Id!.Value,
                Role = FamilyRole.Manager
            };

            _context.FamilyUsers.Add(familyUser);
            await _context.SaveChangesAsync(cancellationToken);

            // Update family stats
            entity.AddDomainEvent(new FamilyStatsUpdatedEvent(entity.Id));

            return Result<Guid>.Success(entity.Id);
        }
        catch (DbUpdateException ex)
        {
            // Log the exception details here if a logger is available
            return Result<Guid>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Database);
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result<Guid>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
        }
    }

    private string GenerateUniqueCode(string prefix)
    {
        // For simplicity, generate a GUID and take a substring.
        // In a real application, you'd want to ensure uniqueness against existing codes in the database.
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
