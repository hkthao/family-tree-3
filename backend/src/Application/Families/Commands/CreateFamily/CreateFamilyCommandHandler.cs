using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Application.Common.Models; // Added for Result<T>
using Microsoft.EntityFrameworkCore; // Added for DbUpdateException

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, Result<Guid>> // Changed return type
{
    private readonly IApplicationDbContext _context;

    public CreateFamilyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken) // Changed return type
    {
        try
        {
            var entity = new Family
            {
                Name = request.Name,
                Description = request.Description,
                Address = request.Address,
                AvatarUrl = request.AvatarUrl,
                Visibility = request.Visibility
            };

            _context.Families.Add(entity);

            // Comment: Write-side invariant: Family is added to the database context.
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(entity.Id); // Wrapped in Result.Success
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