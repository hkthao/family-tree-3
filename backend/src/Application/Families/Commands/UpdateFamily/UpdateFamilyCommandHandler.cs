using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added for Result

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandler : IRequestHandler<UpdateFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateFamilyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                return Result.Failure($"Family with ID {request.Id} not found.", "NotFound");
            }

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.Address = request.Address;
            entity.AvatarUrl = request.AvatarUrl;
            entity.Visibility = request.Visibility;

            // Comment: Write-side invariant: Family is updated in the database context.
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure($"Database error occurred while updating family: {ex.Message}", "Database");
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure($"An unexpected error occurred while updating family: {ex.Message}", "Exception");
        }
    }
}